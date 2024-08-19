<script lang="ts">
	import Login from '$lib/components/Login.svelte';
	import Map from '$lib/components/Map.svelte';
	import { createMapStore, MAPSTORE_CONTEXT_KEY } from '$lib/stores';
	import { setContext } from 'svelte';
	import stravaImage from '$lib/assets/api_logo_pwrdBy_strava_horiz_white.png';

	const mapStore = createMapStore();
	setContext(MAPSTORE_CONTEXT_KEY, mapStore);
</script>

<Map />
<Login />
<img src={stravaImage} alt="Powered by Strava" class="absolute bottom-0 left-0 z-10 h-10" />
