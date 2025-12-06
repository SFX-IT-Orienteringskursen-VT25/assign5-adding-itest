import { getDb, sql } from './db.js';
import { normalizeToNumbers } from './utils.js';

export async function getBucketDb(key) {
  const pool = await getDb();
  const result = await pool.request()
    .input('key', sql.NVarChar(256), key)
    .query(`
      SELECT [sum] FROM dbo.buckets WHERE [key] = @key;
      SELECT CAST([value] AS DECIMAL(18,4)) AS value
      FROM dbo.bucket_entries WHERE [key] = @key ORDER BY id;
    `);

  const sumRow = result.recordsets[0][0];
  if (!sumRow) return null;

  return {
    key,
    values: result.recordsets[1].map(r => Number(r.value)),
    sum: Number(sumRow.sum),
  };
}

export async function getAllBucketsDb() {
  const pool = await getDb();
  const result = await pool.request().query(`
    SELECT [key], [sum] FROM dbo.buckets ORDER BY [key];
  `);

  return {
    count: result.recordset.length,
    buckets: result.recordset.map(row => ({
      key: row.key,
      sum: Number(row.sum),
    })),
  };
}

export async function appendNumbersDb(key, incoming) {
  const nums = normalizeToNumbers(incoming);
  if (nums.length === 0) return { error: 'No valid numbers to append' };

  const pool = await getDb();
  const tx = new sql.Transaction(pool);
  await tx.begin();

  try {
    await new sql.Request(tx)
      .input('key', sql.NVarChar(256), key)
      .query(`
        IF NOT EXISTS (SELECT 1 FROM dbo.buckets WHERE [key] = @key)
          INSERT dbo.buckets([key],[sum]) VALUES(@key, 0);
      `);

    let delta = 0;
    for (const n of nums) {
      delta += n;
      await new sql.Request(tx)
        .input('key', sql.NVarChar(256), key)
        .input('val', sql.Decimal(18, 4), n)
        .query(`
          INSERT dbo.bucket_entries([key],[value]) VALUES(@key, @val);
        `);
    }

    const out = await new sql.Request(tx)
      .input('key', sql.NVarChar(256), key)
      .input('delta', sql.Decimal(18, 4), delta)
      .query(`
        UPDATE dbo.buckets
          SET [sum] = [sum] + @delta, updated_at = SYSUTCDATETIME()
        WHERE [key] = @key;

        SELECT [sum] FROM dbo.buckets WHERE [key] = @key;
        SELECT CAST([value] AS DECIMAL(18,4)) AS value
        FROM dbo.bucket_entries WHERE [key] = @key ORDER BY id;
      `);

    await tx.commit();

    return {
      key,
      values: out.recordsets[1].map(r => Number(r.value)),
      sum: Number(out.recordsets[0][0].sum),
      appended: nums,
    };
  } catch (e) {
    await tx.rollback();
    return { error: e.message };
  }
}
