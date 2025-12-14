const request = require('supertest');
const app = require('../server');
const { getPool, sql } = require('../db/connection');

describe('Storage API Endpoints with Database', () => {
    let pool;

    beforeAll(async () => {
        pool = await getPool();
    });

    beforeEach(async () => {
        // Clear the storage table before each test
        const request = pool.request();
        await request.query('DELETE FROM StorageDB.dbo.Storage');
    });

    afterAll(async () => {
        if (pool) {
            await pool.close();
        }
    });

    describe('POST /api/storage/:key (setItem equivalent)', () => {
        test('should create new item in database and return 201', async () => {
            const response = await request(app)
                .post('/api/storage/testKey')
                .send({ value: 'testValue' })
                .expect(201);
            
            expect(response.body.key).toBe('testKey');
            expect(response.body.value).toBe('testValue');
            
            // Verify it's actually in the database
            const dbRequest = pool.request();
            const result = await dbRequest
                .input('key', sql.NVarChar, 'testKey')
                .query('SELECT value FROM StorageDB.dbo.Storage WHERE [key] = @key');
            
            expect(result.recordset[0].value).toBe('testValue');
        });

        test('should update existing item in database and return 200', async () => {
            // First create an item
            await request(app)
                .post('/api/storage/testKey')
                .send({ value: 'initialValue' });
            
            // Then update it
            const response = await request(app)
                .post('/api/storage/testKey')
                .send({ value: 'updatedValue' })
                .expect(200);
            
            expect(response.body.value).toBe('updatedValue');
            
            // Verify update in database
            const dbRequest = pool.request();
            const result = await dbRequest
                .input('key', sql.NVarChar, 'testKey')
                .query('SELECT value FROM StorageDB.dbo.Storage WHERE [key] = @key');
            
            expect(result.recordset[0].value).toBe('updatedValue');
        });
    });

    describe('GET /api/storage/:key (getItem equivalent)', () => {
        test('should retrieve existing item from database and return 200', async () => {
            // First create an item
            await request(app)
                .post('/api/storage/testKey')
                .send({ value: 'testValue' });
            
            // Then retrieve it
            const response = await request(app)
                .get('/api/storage/testKey')
                .expect(200);
            
            expect(response.body.value).toBe('testValue');
        });

        test('should return 404 for non-existent key in database', async () => {
            const response = await request(app)
                .get('/api/storage/nonExistentKey')
                .expect(404);
            
            expect(response.body.error).toBe('Key not found');
        });
    });
});