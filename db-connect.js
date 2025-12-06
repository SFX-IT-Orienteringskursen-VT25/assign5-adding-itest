import sql from "mssql";
import { getConfig } from "./db.js";

export async function connectWithRetry(retries = 10, delay = 3000) {
    while (retries > 0) {
        try {
            console.log(`Attempting MSSQL connection... (${retries} retries left)`);
            const pool = await sql.connect(getConfig());

            console.log("✔ MSSQL connected");
            return pool;
        } catch (err) {
            console.log("MSSQL connection failed, retrying in 3s...");
            await new Promise((resolve) => setTimeout(resolve, delay));
            retries--;
        }
    }

    throw new Error("Could not connect to MSSQL after multiple attempts");
}
