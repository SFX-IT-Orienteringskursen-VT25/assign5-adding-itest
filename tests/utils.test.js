import { describe, it, expect, beforeEach } from 'vitest';
import {
  getOrInitBucket,
  normalizeToNumbers,
  appendNumbers
} from '../utils.js';

describe('utils.js', () => {
  let store;

  beforeEach(() => {
    store = new Map();
  });

  describe('getOrInitBucket', () => {
    it('initializes a bucket if missing', () => {
      const result = getOrInitBucket(store, 'myKey');

      expect(result).toEqual({ values: [], sum: 0 });
      expect(store.get('myKey')).toEqual({ values: [], sum: 0 });
    });

    it('returns existing bucket', () => {
      store.set('myKey', { values: [5], sum: 5 });

      const result = getOrInitBucket(store, 'myKey');
      expect(result).toEqual({ values: [5], sum: 5 });
    });
  });

  describe('normalizeToNumbers', () => {
    it('converts strings to numbers', () => {
      expect(normalizeToNumbers(['1', '2'])).toEqual([1, 2]);
    });

    it('keeps numbers as numbers', () => {
      expect(normalizeToNumbers([5, 10])).toEqual([5, 10]);
    });

    it('filters out non-numeric values', () => {
      expect(normalizeToNumbers(['1', 'abc', 8])).toEqual([1, 8]);
    });

    it('wraps single value in array', () => {
      expect(normalizeToNumbers(7)).toEqual([7]);
    });
  });

  describe('appendNumbers', () => {
    it('appends valid numbers and updates sum', () => {
      const result = appendNumbers(store, 'keyA', [5, '10']);

      expect(result.appended).toEqual([5, 10]);

      const bucket = store.get('keyA');
      expect(bucket.values).toEqual([5, 10]);
      expect(bucket.sum).toBe(15);
    });

    it('returns error on invalid inputs', () => {
      const result = appendNumbers(store, 'keyA', ['abc']);
      expect(result).toEqual({ error: 'No valid numbers to append' });
    });
  });
});
