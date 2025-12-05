import 'dotenv/config';
import sql from 'mssql';

export const config = {
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

let pool;

export async function getDb() {
    if (!pool) {
        pool = await sql.connect(config);
        console.log(
            `MSSQL connected: ${config.server}:${config.port} (db=${config.database})`
        );
    }
    return pool;
}

export { sql };

process.on('SIGINT', async () => {
    if (pool) {
        await pool.close();
        console.log('MSSQL pool closed');
    }
    process.exit(0);
});
