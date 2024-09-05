<script lang="ts">
	import { dev } from '$app/environment';
	import { goto } from '$app/navigation';
	import stravaButton from '$lib/assets/btn_strava_connectwith_orange.png';
	import { activeSession } from '$lib/stores';
	import { createDialog, melt } from '@melt-ui/svelte';
	import { onMount } from 'svelte';

	const {
		elements: { overlay, content, title, description, close, portalled },
		states: { open }
	} = createDialog({
		role: 'alertdialog',
		forceVisible: true
	});

	const callBackUri = dev ? 'http://localhost:5173/' : 'https://peakshunters.erikmagnusson.com/';
	const apiUrl = dev ? 'http://localhost:7071/api/' : 'https://geo-api.erikmagnusson.com/api/';

	activeSession.subscribe((value) => {
		$open = !value;
	});

	function getCookie(name: string) {
		const value = `; ${document.cookie}`;
		const parts = value.split(`; ${name}=`);
		if (parts.length === 2) return parts?.pop()?.split(';').shift();
	}

	async function readCode() {
		const urlParams = new URLSearchParams(window.location.search);
		const hasCode = urlParams.has('code');

		if (hasCode) {
			$open = false;
			var code = urlParams.get('code');
			await fetch(`${apiUrl}${code}/login`, { method: 'POST', credentials: 'include' }).then(() => {
				goto('/');
			});
		}
	}

	onMount(async () => {
		await readCode();
		if (getCookie('session')) {
			activeSession.set(true);
		} else {
			activeSession.set(false);
		}
	});
</script>

{#if $open}
	<div class="" use:melt={$portalled}>
		<div use:melt={$overlay} class="fixed inset-0 z-50 bg-black/50" />
		<div
			class="fixed left-1/2 top-1/2 z-50 max-h-[85vh] w-[90vw]
            max-w-[450px] -translate-x-1/2 -translate-y-1/2 rounded-md bg-white
            p-6 shadow-lg"
			use:melt={$content}
		>
			<h2 use:melt={$title} class="m-0 text-lg font-medium text-black">Login at Strava</h2>
			<p use:melt={$description} class="mb-5 mt-2 leading-normal text-zinc-600">
				To use PeaksHunters you need to authenticate with Strava. We fetch all your activities from
				strava in order to calculate which peaks you've summited.
			</p>

			<div class="mt-6 flex justify-center">
				<a
					href="https://www.strava.com/oauth/authorize?client_id=26280&response_type=code&redirect_uri={callBackUri}&scope=activity:read"
				>
					<img src={stravaButton} alt="Connect with strava" class="inline-flex" />
				</a>
			</div>

			<button
				use:melt={$close}
				aria-label="Close"
				class="absolute right-[10px] top-[10px] inline-flex h-6 w-6
                appearance-none items-center justify-center rounded-full text-magnum-800
                hover:bg-magnum-100 focus:shadow-magnum-400"
			>
				X
			</button>
		</div>
	</div>
{/if}
