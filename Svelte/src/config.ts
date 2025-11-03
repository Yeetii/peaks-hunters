import { dev } from '$app/environment';

export const config = {
	callBackUri: dev ? 'http://localhost:5173/' : 'https://peakshunters.erikmagnusson.com/',
	callBackUriAuth: dev
		? 'http://localhost:5173/authenticate'
		: 'https://peakshunters.erikmagnusson.com/authenticate',
	apiUrl: dev ? 'http://localhost:7071/api/' : 'https://geo-api.erikmagnusson.com/api/'
};
