<script lang="ts">
	import { PUBLIC_MAPTILER_KEY } from '$env/static/public';
	import summitedPeakIcon from '$lib/assets/peak-summited.png';
	import peakIcon from '$lib/assets/peak.png';
	import Sidebar from '$lib/components/Sidebar.svelte';
	import type { MapStore } from '$lib/stores';
	import { activeSession, MAPSTORE_CONTEXT_KEY } from '$lib/stores';
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

	let mapStore: MapStore = getContext(MAPSTORE_CONTEXT_KEY);

	let mapContainer: HTMLDivElement;

	$: ({ peaks, summitedPeaks } = $peaksStore);

	let leftSidebarCollapsed = true;
	const minFetchPeakZoom = 6.5;

	activeSession.subscribe((active) => {
		console.log('active', active);
	});

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
				clusterRadius: 100
			});

			map.addSource('summitedPeaks', {
				type: 'geojson',
				data: summitedPeaks,
				cluster: true,
				clusterMaxZoom: 7,
				clusterRadius: 100
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
				let groups: Record<string, boolean> = JSON.parse(e.features?.at(0)?.properties['groups']);
				let groupNames = Object.keys(groups);
				const description =
					groupNames.length > 0
						? '<b>Groups:</b> ' + groupNames.join(', ')
						: "Peak doesn't belong to any groups";

				const coordinates = e.lngLat;

				// Ensure that if the map is zoomed out such that multiple
				// copies of the feature are visible, the popup appears
				// over the copy being pointed to.
				while (Math.abs(e.lngLat.lng - coordinates.lng) > 180) {
					coordinates.lng += e.lngLat.lng > coordinates.lng ? 360 : -360;
				}

				new maplibregl.Popup()
					.setLngLat(new LngLat(coordinates.lng, coordinates.lat))
					.setHTML(description)
					.addTo(map);
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

			map.on('zoom', () => {
				console.log('zoom', map.getZoom());
			});

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
