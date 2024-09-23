import { browser, dev } from '$app/environment';
import * as signalR from '@microsoft/signalr';
import { writable } from 'svelte/store';
import { peaksStore } from './peaksStore';

const apiUrl = dev ? 'http://localhost:7071/api/' : 'https://geo-api.erikmagnusson.com/api/';

const createSignalRStore = () => {
	const { subscribe, update } = writable({
		activitiesProcessed: 0,
		lastEventReceived: ''
	});
	return { subscribe, update };
};

export const signalRStore = createSignalRStore();

async function getSignalRAccessInfo() {
	const session = document.cookie
		.split('; ')
		.find((row) => row.startsWith('session='))
		?.split('=')[1];

	if (!session) {
		console.log('No active session, cannot connect to SignalR');
		return;
	}

	const response = await fetch(`${apiUrl}connectSignalR`, {
		headers: {
			session: session
		}
	});
	if (!response.ok) {
		throw new Error('Failed to fetch SignalR access info');
	}
	return response.json();
}

const connect = async () => {
	if (!browser) return; // Svelte dev backend should not connect to SignalR
	try {
		const { url, accessToken } = await getSignalRAccessInfo();

		const connection = new signalR.HubConnectionBuilder()
			.withUrl(url, {
				accessTokenFactory: () => accessToken
			})
			.configureLogging(signalR.LogLevel.Information)
			.build();

		connection.onclose(() => {
			console.log('SignalR connection disconnected');
			setTimeout(() => connect(), 2000);
		});

		connection.on('summitsEvents', (json) => {
			console.log('Summits Event received ', json);
			signalRStore.update((store) => {
				store.activitiesProcessed++;
				store.lastEventReceived = json.summitedPeakNames.toString();
				return store;
			});
		});

		connection.on('summitedPeak', (json) => {
			console.log('Summited peak received ', json);
			peaksStore.addSummitedPeak(json);
		});

		connection.start().then(() => {
			console.log('SignalR connection established');
		});
	} catch (error) {
		console.error('Failed to connect to SignalR:', error);
		setTimeout(() => connect(), 5000); // Retry after 5 seconds
	}
};

connect();
