const request = require('supertest');
const { getPool, sql } = require('../db/connection');
const setup = require('./setup');

let server;
let app;
let dbPool;

describe('Storage API Integration Tests', () => {
  beforeAll(async () => {
    // Set test environment
    process.env.NODE_ENV = 'test';
    
    // Start the server
    app = require('../server');
    server = app.listen(4000); // Different port for testing
    
    // Get database connection
    dbPool = await getPool();
    
    // Clear any existing test data
    await dbPool.request().query('DELETE FROM TestStorageDB.dbo.Storage');
  }, 30000); // Increased timeout for Docker setup

  afterAll(async () => {
    // Clean up database
    if (dbPool) {
      await dbPool.request().query('DELETE FROM TestStorageDB.dbo.Storage');
    }
    
    // Close server
    if (server) {
      server.close();
    }
    
    // Run cleanup from setup
    if (typeof setup.cleanup === 'function') {
      await setup.cleanup();
    }
  }, 30000);

  beforeEach(async () => {
    // Clear storage table before each test
    await dbPool.request().query('DELETE FROM TestStorageDB.dbo.Storage');
  });

  describe('POST /api/storage/:key', () => {
    test('should create new item and return 201 (Positive Test)', async () => {
      const response = await request(server)
        .post('/api/storage/testKey')
        .send({ value: 'testValue' })
        .expect(201);
      
      expect(response.body).toEqual({
        key: 'testKey',
        value: 'testValue',
        message: 'Item created successfully'
      });
    });

    test('should return 400 when value is missing (Negative Test)', async () => {
      const response = await request(server)
        .post('/api/storage/testKey')
        .send({})
        .expect(400);
      
      expect(response.body.error).toBe('Bad Request');
      expect(response.body.message).toBe('Value is required in request body');
    });

    test('should update existing item and return 200', async () => {
      // First create
      await request(server)
        .post('/api/storage/existingKey')
        .send({ value: 'initial' })
        .expect(201);
      
      // Then update
      const response = await request(server)
        .post('/api/storage/existingKey')
        .send({ value: 'updated' })
        .expect(200);
      
      expect(response.body.value).toBe('updated');
      expect(response.body.message).toBe('Item updated successfully');
    });

    test('should handle special characters in key and value', async () => {
      const key = 'key-with-dash_and_underscore';
      const value = '{"nested": {"data": "test"}, "array": [1,2,3]}';
      
      const response = await request(server)
        .post(`/api/storage/${key}`)
        .send({ value: value })
        .expect(201);
      
      expect(response.body.key).toBe(key);
      expect(response.body.value).toBe(value);
    });
  });

  describe('GET /api/storage/:key', () => {
    test('should retrieve existing item (Positive Test)', async () => {
      // First create
      await request(server)
        .post('/api/storage/retrieveKey')
        .send({ value: 'retrieveValue' })
        .expect(201);
      
      // Then retrieve
      const response = await request(server)
        .get('/api/storage/retrieveKey')
        .expect(200);
      
      expect(response.body.key).toBe('retrieveKey');
      expect(response.body.value).toBe('retrieveValue');
    });

    test('should return 404 for non-existent key (Negative Test)', async () => {
      const response = await request(server)
        .get('/api/storage/nonexistent')
        .expect(404);
      
      expect(response.body.error).toBe('Key not found');
    });

    test('should retrieve item with JSON value', async () => {
      const jsonValue = JSON.stringify({ name: 'John', age: 30 });
      
      await request(server)
        .post('/api/storage/jsonKey')
        .send({ value: jsonValue })
        .expect(201);
      
      const response = await request(server)
        .get('/api/storage/jsonKey')
        .expect(200);
      
      expect(response.body.value).toBe(jsonValue);
    });
  });

  describe('DELETE /api/storage/:key', () => {
    test('should delete existing item (Positive Test)', async () => {
      // Create first
      await request(server)
        .post('/api/storage/deleteKey')
        .send({ value: 'toDelete' })
        .expect(201);
      
      // Delete
      const response = await request(server)
        .delete('/api/storage/deleteKey')
        .expect(200);
      
      expect(response.body.message).toBe('Item deleted successfully');
      
      // Verify deletion
      await request(server)
        .get('/api/storage/deleteKey')
        .expect(404);
    });

    test('should return 404 when deleting non-existent key (Negative Test)', async () => {
      const response = await request(server)
        .delete('/api/storage/nonexistent')
        .expect(404);
      
      expect(response.body.error).toBe('Key not found');
    });
  });

  describe('GET /api/storage', () => {
    test('should return all items', async () => {
      // Create multiple items
      await request(server)
        .post('/api/storage/key1')
        .send({ value: 'value1' });
      
      await request(server)
        .post('/api/storage/key2')
        .send({ value: 'value2' });
      
      await request(server)
        .post('/api/storage/key3')
        .send({ value: 'value3' });
      
      const response = await request(server)
        .get('/api/storage')
        .expect(200);
      
      expect(response.body.count).toBe(3);
      expect(response.body.items.key1).toBe('value1');
      expect(response.body.items.key2).toBe('value2');
      expect(response.body.items.key3).toBe('value3');
    });

    test('should return empty object when no items exist', async () => {
      const response = await request(server)
        .get('/api/storage')
        .expect(200);
      
      expect(response.body.count).toBe(0);
      expect(Object.keys(response.body.items)).toHaveLength(0);
    });
  });

  describe('Concurrency Tests', () => {
    test('should handle simultaneous requests correctly', async () => {
      const key = 'concurrentKey';
      
      // Make multiple simultaneous requests
      const promises = [
        request(server).post(`/api/storage/${key}`).send({ value: 'value1' }),
        request(server).post(`/api/storage/${key}`).send({ value: 'value2' }),
        request(server).post(`/api/storage/${key}`).send({ value: 'value3' })
      ];
      
      const results = await Promise.allSettled(promises);
      
      // One should succeed, others might fail due to concurrency
      const successful = results.filter(r => r.status === 'fulfilled' && r.value.status === 201);
      expect(successful.length).toBeGreaterThanOrEqual(1);
    });
  });

  describe('Error Handling Tests', () => {
    test('should handle database connection errors gracefully', async () => {
      // Temporarily close database connection
      const originalPool = dbPool;
      const mockPool = {
        request: () => {
          throw new Error('Database connection lost');
        }
      };
      
      // Temporarily replace getPool function
      const originalGetPool = require('../db/connection').getPool;
      require('../db/connection').getPool = () => Promise.resolve(mockPool);
      
      const response = await request(server)
        .get('/api/storage/someKey')
        .expect(500);
      
      expect(response.body.error).toBe('Internal Server Error');
      
      // Restore original function
      require('../db/connection').getPool = originalGetPool;
    });
  });
});