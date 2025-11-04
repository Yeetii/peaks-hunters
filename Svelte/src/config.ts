import { dev } from '$app/environment';
import { env } from '$env/dynamic/public';

export const config = {
	callBackUri: dev ? 'http://localhost:5173/' : 'https://peakshunters.erikmagnusson.com/',
	callBackUriAuth: dev
		? 'http://localhost:5173/authenticate'
		: 'https://peakshunters.erikmagnusson.com/authenticate',
	apiUrl: env.PUBLIC_VITE_API_URL ?? 'https://geo-api.erikmagnusson.com/api/'
};
