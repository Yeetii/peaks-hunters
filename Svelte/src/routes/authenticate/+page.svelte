<script lang="ts">
	import { dev } from '$app/environment';
	import { goto } from '$app/navigation';
	import { onMount } from 'svelte';

	const callBackUri = dev
		? 'http://localhost:5173/authenticate'
		: 'https://peakshunters.erikmagnusson.com/authenticate';
	const apiUrl = dev ? 'http://localhost:7071/api/' : 'https://geo-api.erikmagnusson.com/api/';
	var fetchFailed = false;
	var waiting = false;

	onMount(() => {
		const urlParams = new URLSearchParams(window.location.search);
		const hasCode = urlParams.has('code');

		if (hasCode) {
			var code = urlParams.get('code');
			waiting = true;
			fetch(`${apiUrl}${code}/login`, { method: 'POST', credentials: 'include' }).then((r) => {
				waiting = false;
				if (!r.ok) {
					fetchFailed = true;
				} else {
					goto('/');
				}
			});
		}
	});
</script>

<h1>Auth site</h1>

{#if waiting}
	<p>Please wait...</p>
{/if}

{#if fetchFailed}
	<p>Fetching access code failed</p>
{/if}

<a
	href="https://www.strava.com/oauth/authorize?client_id=26280&response_type=code&redirect_uri={callBackUri}&scope=activity:read"
	>Go to strava</a
>
