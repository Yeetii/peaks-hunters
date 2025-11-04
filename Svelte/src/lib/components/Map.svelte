<script lang="ts">
	import { PUBLIC_MAPTILER_KEY } from '$env/static/public';
	import summitedPeakIcon from '$lib/assets/peak-summited.png';
	import peakIcon from '$lib/assets/peak.png';
	import Sidebar from '$lib/components/Sidebar.svelte';
	import type { MapStore } from '$lib/stores';
	import { MAPSTORE_CONTEXT_KEY } from '$lib/stores';
	import { peaksStore } from '$lib/stores/peaksStore';
	import maplibregl, {
		AttributionControl,
		GeolocateControl,
		LngLat,
		Map,
		NavigationControl,
		ScaleControl
	} from 'maplibre-gl';
	import 'maplibre-gl/dist/maplibre-gl.css';
	import { getContext, onMount } from 'svelte';
	import '../../global.css';

	let mapStore: MapStore = getContext(MAPSTORE_CONTEXT_KEY);

	let mapContainer: HTMLDivElement;

	$: ({ peaks, summitedPeaks } = $peaksStore);

	let leftSidebarCollapsed = true;
	const minFetchPeakZoom = 6.5;

	function handleSidebarToggle(event: CustomEvent) {
		const { side, collapsed } = event.detail;
		if (side === 'left') {
			leftSidebarCollapsed = collapsed;
		}

		mapStore?.update((map) => {
			if (map) {
				const padding = {
					left: leftSidebarCollapsed ? 0 : 300
				};
				map.easeTo({ padding, duration: 1000 });
			}
			return map;
		});
	}

	onMount(() => {
		// Guard: when running in SSR (e.g. during certain test configurations) skip map initialization.
		// Vitest jsdom should have window defined; if not, we avoid calling MapLibre which requires real DOM/WebGL.
		if (typeof window === 'undefined') {
			return;
		}
		var center = new LngLat(13.0509, 63.41698);
		var zoom = 12;
		const mapCenter = localStorage.getItem('mapCenter');
		const mapZoom = localStorage.getItem('mapZoom');
		if (mapCenter && mapZoom) {
			center = JSON.parse(mapCenter);
			zoom = parseFloat(mapZoom);
		}

		const map = new Map({
			container: mapContainer,
			style: `https://api.maptiler.com/maps/c852a07e-70f5-49c3-aebf-ad7d488e4495/style.json?key=${PUBLIC_MAPTILER_KEY}`,
			center: center,
			zoom: zoom,
			hash: true,
			attributionControl: false
		});
		map.addControl(new NavigationControl({}), 'top-right');
		map.addControl(
			new GeolocateControl({
				positionOptions: { enableHighAccuracy: true },
				trackUserLocation: true
			}),
			'top-right'
		);
		map.addControl(new ScaleControl({ maxWidth: 80, unit: 'metric' }), 'bottom-right');
		map.addControl(new AttributionControl({ compact: true }), 'bottom-right');

		mapStore?.set(map);

		map.on('error', (e: Error) => {
			console.error('Map error: ', e);
		});

		map.on('load', () => {
			map.loadImage(peakIcon).then((image) => map.addImage('peakIcon', image.data));
			map.loadImage(summitedPeakIcon).then((image) => map.addImage('summitedPeakIcon', image.data));

			map.addControl(
				new maplibregl.TerrainControl({
					source: 'terrain_rgb',
					exaggeration: 1.5
				})
			);

			map.addSource('peaks', {
				type: 'geojson',
				data: peaks,
				cluster: true,
				clusterMaxZoom: 10,
				clusterRadius: 80
			});

			map.addSource('summitedPeaks', {
				type: 'geojson',
				data: summitedPeaks,
				cluster: true,
				clusterMaxZoom: 8,
				clusterRadius: 40
			});

			map.addLayer({
				id: 'peaks',
				type: 'symbol',
				source: 'peaks',
				minzoom: 5,
				layout: {
					'icon-image': 'peakIcon',
					'text-field': [
						'concat',
						[
							'case',
							['has', 'cluster'],
							['concat', ['get', 'point_count'], ' peaks'],
							[
								'concat',
								['get', 'name'],
								'\n',
								[
									'case',
									['has', 'elevation'],
									['concat', ['to-string', ['get', 'elevation']], ' m'],
									''
								]
							]
						]
					],
					'text-font': ['Roboto Regular', 'Noto Sans Regular'],
					'text-offset': [0, 1.25],
					'text-anchor': 'top',
					'icon-size': 0.75
				}
			});

			map.addLayer({
				id: 'summitedPeaks',
				type: 'symbol',
				source: 'summitedPeaks',
				layout: {
					'icon-image': 'summitedPeakIcon',
					'text-field': [
						'concat',
						[
							'case',
							['has', 'cluster'],
							['concat', ['get', 'point_count'], ' peaks'],
							[
								'concat',
								['get', 'name'],
								'\n',
								[
									'case',
									['has', 'elevation'],
									['concat', ['to-string', ['get', 'elevation']], ' m'],
									''
								]
							]
						]
					],
					'text-font': ['Roboto Regular', 'Noto Sans Regular'],
					'text-offset': [0, 1.25],
					'text-anchor': 'top',
					'icon-size': 0.75
				}
			});

			function handlePopup(
				e: maplibregl.MapMouseEvent & { features?: maplibregl.MapGeoJSONFeature[] }
			) {
				if (!e.features || e.features.length === 0) return;

				const feature = e.features[0];

				// Check if it's a cluster
				if (feature.properties?.cluster) {
					map.easeTo({
						center: [e.lngLat.lng, e.lngLat.lat],
						zoom: map.getZoom() + 2,
						duration: 1000
					});
				} else {
					// Zoom to individual feature
					const geometry = feature.geometry as { type: 'Point'; coordinates?: [number, number] };
					const center = geometry.coordinates ?? [e.lngLat.lng, e.lngLat.lat];
					map.easeTo({
						center: center,
						zoom: Math.max(map.getZoom(), 13),
						duration: 1000
					});
				}
			}

			map.on('click', 'peaks', handlePopup);
			map.on('click', 'summitedPeaks', handlePopup);

			map.on('mouseenter', 'peaks', () => {
				map.getCanvas().style.cursor = 'pointer';
			});

			map.on('mouseenter', 'summitedPeaks', () => {
				map.getCanvas().style.cursor = 'pointer';
			});

			map.on('mouseleave', 'peaks', () => {
				map.getCanvas().style.cursor = '';
			});

			map.on('mouseleave', 'summitedPeaks', () => {
				map.getCanvas().style.cursor = '';
			});

			peaksStore.fetchPeaks(map.getCenter());

			map.on('move', () => {
				if (map.getZoom() > minFetchPeakZoom) {
					peaksStore.fetchPeaks(map.getCenter());
				}
			});

			map.on('moveend', () => {
				localStorage.setItem('mapCenter', JSON.stringify(map.getCenter()));
				localStorage.setItem('mapZoom', map.getZoom().toString());
			});
		});
	});
</script>

<div class="map w-full h-full" data-testid="map" bind:this={mapContainer}>
	<Sidebar
		side="left"
		content="Waiting for stats..."
		bind:collapsed={leftSidebarCollapsed}
		on:toggle={handleSidebarToggle}
	/>
</div>

<style>
	.map {
		height: 100vh;
		width: 100vw;
	}
</style>
