import { beforeAll, afterAll, describe, it, expect } from "vitest";
import request from "supertest";
import { MSSQLServerContainer } from "@testcontainers/mssqlserver";

let container;
let app;

beforeAll(
    async () => {
        container = await new MSSQLServerContainer("mcr.microsoft.com/mssql/server:2022-latest")
            .acceptLicense()
            .withEnvironment({
                SA_PASSWORD: "Test!12345",
            })
            .start();

        process.env.DB_HOST = container.getHost();
        process.env.DB_PORT = String(container.getPort());
        process.env.DB_USER = "sa";
        process.env.DB_PASSWORD = "Test!12345";
        process.env.DB_NAME = "appdb_test";

        const { resetPool } = await import("../db.js");
        await resetPool();

        const { initDb } = await import("../db-init.js");
        await initDb();

        const appModule = await import("../app.js");
        app = appModule.default;
    },
    180_000
);


afterAll(async () => {
    const { resetPool } = await import("../db.js");
    await resetPool();

    if (container) {
        await container.stop();
    }
});

describe("GET /db-version", () => {
    it("returns SQL Server version", async () => {
        const res = await request(app).get("/db-version");

        expect(res.status).toBe(200);
        expect(res.body).toHaveProperty("version");
        expect(typeof res.body.version).toBe("string");
    });
});

describe("Storage API — positive cases", () => {
    it("returns empty list when no buckets exist", async () => {
        const res = await request(app).get("/storage");

        expect(res.status).toBe(200);
        expect(res.body).toHaveProperty("count");
        expect(res.body).toHaveProperty("buckets");
        expect(Array.isArray(res.body.buckets)).toBe(true);
    });

    it("creates a bucket and appends numbers", async () => {
        const res = await request(app)
            .post("/storage/mybucket")
            .send({ value: [1, 2, 3] });

        expect(res.status).toBe(200);
        expect(res.body).toMatchObject({
            key: "mybucket",
            values: [1, 2, 3],
            sum: 6,
            appended: [1, 2, 3],
        });

        const fetchRes = await request(app).get("/storage/mybucket");

        expect(fetchRes.status).toBe(200);
        expect(fetchRes.body).toMatchObject({
            key: "mybucket",
            values: [1, 2, 3],
            sum: 6,
        });
    });
});

describe("Storage API — negative cases", () => {
    it("returns 400 if 'value' is missing in POST body", async () => {
        const res = await request(app)
            .post("/storage/missingvalue")
            .send({});

        expect(res.status).toBe(400);
        expect(res.body.error).toContain('Missing "value"');
    });

    it("returns 404 when bucket does not exist", async () => {
        const res = await request(app).get("/storage/unknown123");

        expect(res.status).toBe(404);
        expect(res.body).toMatchObject({
            error: "Not found",
            key: "unknown123",
        });
    });

    it("returns 400 when no valid numbers can be parsed", async () => {
        const res = await request(app)
            .post("/storage/badvalues")
            .send({ value: ["not-a-number", "abc"] });

        expect(res.status).toBe(400);
        expect(res.body.error).toBe("No valid numbers to append");
    });
});
