import sql from 'mssql';

if (process.env.NODE_ENV !== 'test') {
    await import('dotenv/config');
}

export function getConfig() {
    return {
        server: process.env.DB_HOST,
        port: parseInt(process.env.DB_PORT, 10),
        user: process.env.DB_USER,
        password: process.env.DB_PASSWORD,
        database: process.env.DB_NAME,
        options: {
            encrypt: false,
            trustServerCertificate: true,
        },
        pool: {
            max: 10,
            min: 0,
            idleTimeoutMillis: 30000,
        },
    };
}

export const config = getConfig();

let pool;

export async function getDb() {
    if (!pool) {
        const cfg = getConfig();
        pool = await sql.connect(cfg);
        console.log(
            `MSSQL connected: ${cfg.server}:${cfg.port} (db=${cfg.database})`
        );
    }
    return pool;
}

export async function resetPool() {
    if (pool) {
        await pool.close();
        pool = null;
    }
    await sql.close();
}

export { sql };

process.on('SIGINT', async () => {
    if (pool) {
        await pool.close();
        console.log('MSSQL pool closed');
    }
    process.exit(0);
});
