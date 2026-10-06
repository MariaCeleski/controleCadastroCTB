import { describe, expect, it } from 'vitest';
import { formatCnpj, isValidCnpj } from './cnpj';

describe('CNPJ helpers', () => {
  it('formats digits for presentation', () =>
    expect(formatCnpj('11222333000181')).toBe('11.222.333/0001-81'));
  it('rejects invalid verification digits', () =>
    expect(isValidCnpj('11.222.333/0001-80')).toBe(false));
  it('accepts a valid CNPJ', () => expect(isValidCnpj('11.222.333/0001-81')).toBe(true));
});
