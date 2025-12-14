const express = require('express');
const router = express.Router();
const { getPool, sql } = require('../db/connection');

// GET /api/storage/:key - Equivalent to localStorage.getItem(key)
router.get('/storage/:key', async (req, res) => {
    const { key } = req.params;
    
    try {
        const pool = await getPool();
        const request = pool.request();
        
        const result = await request
            .input('key', sql.NVarChar, key)
            .query('SELECT value FROM StorageDB.dbo.Storage WHERE [key] = @key');
        
        if (result.recordset.length > 0) {
            return res.status(200).json({
                key: key,
                value: result.recordset[0].value,
                message: 'Item retrieved successfully'
            });
        } else {
            return res.status(404).json({
                error: 'Key not found',
                message: `No item found with key: ${key}`
            });
        }
    } catch (error) {
        console.error('Database error:', error);
        return res.status(500).json({
            error: 'Internal Server Error',
            message: 'Failed to retrieve item from database'
        });
    }
});

// POST /api/storage/:key - Equivalent to localStorage.setItem(key, value)
router.post('/storage/:key', async (req, res) => {
    const { key } = req.params;
    const { value } = req.body;
    
    if (value === undefined) {
        return res.status(400).json({
            error: 'Bad Request',
            message: 'Value is required in request body'
        });
    }
    
    try {
        const pool = await getPool();
        const request = pool.request();
        
        // Check if key exists
        const checkResult = await request
            .input('key', sql.NVarChar, key)
            .query('SELECT value FROM StorageDB.dbo.Storage WHERE [key] = @key');
        
        const exists = checkResult.recordset.length > 0;
        
        if (exists) {
            // Update existing record
            await request
                .input('key', sql.NVarChar, key)
                .input('value', sql.NVarChar, value)
                .query(`
                    UPDATE StorageDB.dbo.Storage 
                    SET value = @value, updated_at = GETDATE() 
                    WHERE [key] = @key
                `);
            
            return res.status(200).json({
                key: key,
                value: value,
                message: 'Item updated successfully'
            });
        } else {
            // Insert new record
            await request
                .input('key', sql.NVarChar, key)
                .input('value', sql.NVarChar, value)
                .query(`
                    INSERT INTO StorageDB.dbo.Storage ([key], value) 
                    VALUES (@key, @value)
                `);
            
            return res.status(201).json({
                key: key,
                value: value,
                message: 'Item created successfully'
            });
        }
    } catch (error) {
        console.error('Database error:', error);
        return res.status(500).json({
            error: 'Internal Server Error',
            message: 'Failed to save item to database'
        });
    }
});

// DELETE /api/storage/:key
router.delete('/storage/:key', async (req, res) => {
    const { key } = req.params;
    
    try {
        const pool = await getPool();
        const request = pool.request();
        
        const result = await request
            .input('key', sql.NVarChar, key)
            .query('DELETE FROM StorageDB.dbo.Storage WHERE [key] = @key');
        
        if (result.rowsAffected[0] > 0) {
            return res.status(200).json({
                message: 'Item deleted successfully',
                key: key
            });
        } else {
            return res.status(404).json({
                error: 'Key not found',
                message: `No item found with key: ${key}`
            });
        }
    } catch (error) {
        console.error('Database error:', error);
        return res.status(500).json({
            error: 'Internal Server Error',
            message: 'Failed to delete item from database'
        });
    }
});

// GET /api/storage - Get all items
router.get('/storage', async (req, res) => {
    try {
        const pool = await getPool();
        const request = pool.request();
        
        const result = await request.query('SELECT [key], value FROM StorageDB.dbo.Storage');
        
        const items = {};
        result.recordset.forEach(row => {
            items[row.key] = row.value;
        });
        
        return res.status(200).json({
            items: items,
            count: result.recordset.length,
            message: 'All storage items retrieved successfully'
        });
    } catch (error) {
        console.error('Database error:', error);
        return res.status(500).json({
            error: 'Internal Server Error',
            message: 'Failed to retrieve items from database'
        });
    }
});

module.exports = router;