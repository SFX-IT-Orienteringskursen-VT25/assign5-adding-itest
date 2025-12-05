export function getOrInitBucket(store, key) {

  if (!store.has(key)) {
    store.set(key, { values: [], sum: 0 });
  }
  return store.get(key);
}

export function normalizeToNumbers(value) {

  const arr = Array.isArray(value) ? value : [value];

  const nums = arr.map((v) => {
    const n = typeof v === "string" ? Number(v) : v;
    return Number(n);
  });


  const valid = nums.filter((n) => Number.isFinite(n));
  return valid;
}

export function appendNumbers(store, key, incoming) {
  const toAppend = normalizeToNumbers(incoming);
  if (toAppend.length === 0) {
    return { error: 'No valid numbers to append' };
  }

  const bucket = getOrInitBucket(store, key);
  for (const n of toAppend) {
    bucket.values.push(n);
    bucket.sum += n;
  }

  store.set(key, bucket);

  return { bucket, appended: toAppend };
}
