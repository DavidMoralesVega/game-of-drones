import { HttpErrorResponse } from '@angular/common/http';
import { errorMessage } from './error-message';

describe('errorMessage', () => {
  it('uses the problem details detail from the API', () => {
    const error = new HttpErrorResponse({
      status: 400,
      error: { detail: 'Rock cannot kill itself.' },
    });

    expect(errorMessage(error)).toBe('Rock cannot kill itself.');
  });

  it('falls back to a generic message for anything else', () => {
    expect(errorMessage(new Error('boom'))).toBe('Something went wrong. Please try again.');
    expect(errorMessage(new HttpErrorResponse({ status: 500 }))).toBe(
      'Something went wrong. Please try again.',
    );
  });
});
