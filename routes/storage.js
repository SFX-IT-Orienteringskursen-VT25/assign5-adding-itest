const express = require('express');
const router = express.Router();

// In-memory storage (replace with database in production)
let storage = {};

// GET /api/storage/:key - Equivalent to localStorage.getItem(key)
router.get('/storage/:key', (req, res) => {
    const { key } = req.params;
    
    if (storage.hasOwnProperty(key)) {
        return res.status(200).json({
            key: key,
            value: storage[key],
            message: 'Item retrieved successfully'
        });
    } else {
        return res.status(404).json({
            error: 'Key not found',
            message: `No item found with key: ${key}`
        });
    }
});

// POST /api/storage/:key - Equivalent to localStorage.setItem(key, value)
router.post('/storage/:key', (req, res) => {
    const { key } = req.params;
    const { value } = req.body;
    
    if (value === undefined) {
        return res.status(400).json({
            error: 'Bad Request',
            message: 'Value is required in request body'
        });
    }
    
    const isNewKey = !storage.hasOwnProperty(key);
    storage[key] = value;
    
    const statusCode = isNewKey ? 201 : 200;
    const message = isNewKey ? 'Item created successfully' : 'Item updated successfully';
    
    return res.status(statusCode).json({
        key: key,
        value: storage[key],
        message: message
    });
});

module.exports = router;