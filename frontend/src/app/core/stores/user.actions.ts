import { createAction, props } from '@ngrx/store';
import { User } from './user.model';

export const setUser = createAction('[User] Set User', props<{ user: User }>());
export const clearUser = createAction('[User] Clear User');
export const updateAccessToken = createAction(
  '[User] Update Access Token',
  props<{ accessToken: string }>(),
);
