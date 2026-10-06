/** Keeps the API contract normalized while the view is free to use a mask. */
export function onlyDigits(value: string): string {
  return value.replace(/\D/g, '');
}

export function formatCnpj(value: string): string {
  // The API stores only digits; this mask exists solely to help people read and type the value.
  const digits = onlyDigits(value).slice(0, 14);
  return digits
    .replace(/^(\d{2})(\d)/, '$1.$2')
    .replace(/^(\d{2}\.\d{3})(\d)/, '$1.$2')
    .replace(/\.(\d{3})(\d)/, '.$1/$2')
    .replace(/(\d{4})(\d)/, '$1-$2');
}

export function isValidCnpj(value: string): boolean {
  const digits = onlyDigits(value);
  if (digits.length !== 14 || /^(\d)\1+$/.test(digits)) return false;
  // Both check digits are derived from the preceding digits according to Brazil's CNPJ algorithm.
  const calculate = (length: number) => {
    let factor = length - 7;
    const sum = digits
      .slice(0, length)
      .split('')
      .reduce((total, digit) => {
        const result = total + Number(digit) * factor;
        factor = factor === 2 ? 9 : factor - 1;
        return result;
      }, 0);
    const remainder = sum % 11;
    return remainder < 2 ? 0 : 11 - remainder;
  };
  return calculate(12) === Number(digits[12]) && calculate(13) === Number(digits[13]);
}
