<script lang="ts">
	import { dev } from '$app/environment';
	import { PUBLIC_MAPTILER_KEY } from '$env/static/public';
	import summitedPeakIcon from '$lib/assets/peak-summited.png';
	import peakIcon from '$lib/assets/peak.png';
	import Sidebar from '$lib/components/Sidebar.svelte';
	import type { MapStore } from '$lib/stores';
	import { activeSession, MAPSTORE_CONTEXT_KEY } from '$lib/stores';
	import type { Feature, FeatureCollection, GeoJSON, GeoJsonProperties, Geometry } from 'geojson';
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

	const apiUrl = dev ? 'http://localhost:7071/api/' : 'https://geo-api.erikmagnusson.com/api/';

	let mapStore: MapStore = getContext(MAPSTORE_CONTEXT_KEY);

	let mapContainer: HTMLDivElement;

	let fetchRadius = 50000;
	let cancelFetchZone = 0.5;

	let queriedLocations = new Array<LngLat>();
	let peaks: GeoJSON.FeatureCollection = {
		type: 'FeatureCollection',
		features: new Array<Feature<Geometry, GeoJsonProperties>>()
	};
	let summitedPeaks: GeoJSON.FeatureCollection = {
		type: 'FeatureCollection',
		features: new Array<Feature<Geometry, GeoJsonProperties>>()
	};
	let peakIds = new Set();

	const R = 6371e3; // Earth's radius in meters

	let leftSidebarCollapsed = true;

	function toRadians(degrees: number): number {
		return degrees * (Math.PI / 180);
	}

	function calculateDistance(p1: LngLat, p2: LngLat): number {
		const φ1 = toRadians(p1.lat);
		const φ2 = toRadians(p2.lat);
		const Δφ = toRadians(p2.lat - p1.lat);
		const Δλ = toRadians(p2.lng - p1.lng);

		const a =
			Math.sin(Δφ / 2) * Math.sin(Δφ / 2) +
			Math.cos(φ1) * Math.cos(φ2) * Math.sin(Δλ / 2) * Math.sin(Δλ / 2);
		const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));

		return R * c; // Distance in meters
	}

	const fetchPeaks = async (center: LngLat): Promise<[peaks: GeoJSON, summitedPeaks: GeoJSON]> => {
		var alreadyFetched = queriedLocations.some((lngLat) => {
			var distance = calculateDistance(lngLat, center);
			return distance < fetchRadius * cancelFetchZone;
		});

		if (alreadyFetched) return [peaks, summitedPeaks];

		queriedLocations.push(center);

		return fetch(`${apiUrl}peaks?lat=${center.lat}&lon=${center.lng}&radius=${fetchRadius}`, {
			credentials: 'include'
		})
			.then((r) => {
				if (r.ok) {
					return r.json();
				}
				if (r.status === 401) {
					activeSession.set(false);
				}
			})
			.then((newPeaks: FeatureCollection) => {
				let filteredPeaks = newPeaks.features.filter((feature) => !peakIds.has(feature.id));
				filteredPeaks.forEach((peak) => peakIds.add(peak.id));
				filteredPeaks.forEach((peak) => {
					if (peak.properties && peak.properties['summited']) {
						summitedPeaks.features.push(peak);
					} else {
						peaks.features.push(peak);
					}
				});
				return [peaks, summitedPeaks];
			});
	};

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
		const map = new Map({
			container: mapContainer,
			style: `https://api.maptiler.com/maps/topo-v2/style.json?key=${PUBLIC_MAPTILER_KEY}`,
			center: [13.0509, 63.41698],
			zoom: 12,
			hash: true,
			attributionControl: false,
			maxZoom: 14
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

		map.on('load', () => {
			map.loadImage(peakIcon).then((image) => map.addImage('peakIcon', image.data));
			map.loadImage(summitedPeakIcon).then((image) => map.addImage('summitedPeakIcon', image.data));

			map.addControl(
				new maplibregl.TerrainControl({
					source: 'terrain_rgb',
					exaggeration: 1.5
				})
			);

			fetchPeaks(map.getCenter()).then(([peaks, summitedPeaks]) => {
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
			});

			map.on('click', 'peaks', (e) => {
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
			});

			map.on('click', 'summitedPeaks', (e) => {
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
			});

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

			map.on('moveend', () => {
				console.log(map.getZoom());
				fetchPeaks(map.getCenter()).then(([peaks, summitedPeaks]) => {
					const peaksSource = map.getSource('peaks') as maplibregl.GeoJSONSource;
					const summitedPeaksSource = map.getSource('summitedPeaks') as maplibregl.GeoJSONSource;
					peaksSource.setData(peaks);
					summitedPeaksSource.setData(summitedPeaks);
				});
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
