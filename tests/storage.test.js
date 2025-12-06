import { describe, it, expect, beforeEach, vi } from 'vitest';
import { appendNumbersDb, getBucketDb, getAllBucketsDb } from '../storage.js';

const mockRequest = {
    input: vi.fn().mockReturnThis(),
    query: vi.fn(),
};

const mockTransaction = {
    begin: vi.fn(),
    commit: vi.fn(),
    rollback: vi.fn(),
};

const mockPool = {
    request: () => mockRequest,
};

vi.mock('../db.js', () => ({
    getDb: vi.fn(async () => mockPool),
    sql: {
        Transaction: vi.fn(() => mockTransaction),
        Request: vi.fn(() => mockRequest),
        NVarChar: vi.fn(),
        Decimal: vi.fn(),
    },
}));

beforeEach(() => {
    vi.clearAllMocks();
});

describe('storage.js', () => {
    it('appendNumbersDb: commits transaction on success', async () => {
        // 1. ensure bucket exists
        mockRequest.query
            .mockResolvedValueOnce({})
            .mockResolvedValueOnce({})
            .mockResolvedValueOnce({})
            .mockResolvedValueOnce({
                recordsets: [
                    [{ sum: 15 }],
                    [{ value: 5 }, { value: 10 }]
                ]
            });

        const result = await appendNumbersDb('testKey', [5, 10]);

        expect(mockTransaction.begin).toHaveBeenCalled();
        expect(mockTransaction.commit).toHaveBeenCalled();

        expect(result).toEqual({
            key: 'testKey',
            appended: [5, 10],
            sum: 15,
            values: [5, 10],
        });
    });

    it('appendNumbersDb: rolls back on error', async () => {
        mockRequest.query.mockRejectedValueOnce(new Error('DB failed'));

        const result = await appendNumbersDb('testKey', [5]);

        expect(mockTransaction.rollback).toHaveBeenCalled();
        expect(result.error).toBe('DB failed');
    });

    it('getBucketDb: returns bucket data when found', async () => {
        mockRequest.query.mockResolvedValueOnce({
            recordsets: [
                [{ sum: 20 }],
                [{ value: 5 }, { value: 15 }]
            ]
        });

        const result = await getBucketDb('abc');

        expect(result).toEqual({
            key: 'abc',
            sum: 20,
            values: [5, 15],
        });
    });

    it('getBucketDb: returns null when no bucket exists', async () => {
        mockRequest.query.mockResolvedValueOnce({
            recordsets: [[]]
        });

        const result = await getBucketDb('missing');

        expect(result).toBeNull();
    });

    it('getAllBucketsDb: returns all bucket summaries', async () => {
        mockRequest.query.mockResolvedValueOnce({
            recordset: [
                { key: 'A', sum: 10 },
                { key: 'B', sum: 25 },
            ],
        });

        const result = await getAllBucketsDb();

        expect(result).toEqual({
            count: 2,
            buckets: [
                { key: 'A', sum: 10 },
                { key: 'B', sum: 25 },
            ],
        });
    });
});
