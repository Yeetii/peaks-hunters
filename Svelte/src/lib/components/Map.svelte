<script lang="ts">
	import { dev } from '$app/environment';
	import { PUBLIC_MAPTILER_KEY } from '$env/static/public';
	import summitedPeakIcon from '$lib/assets/peak-summited.png';
	import peakIcon from '$lib/assets/peak.png';
	import type { MapStore } from '$lib/stores';
	import { activeSession, MAPSTORE_CONTEXT_KEY } from '$lib/stores';
	import type { FeatureCollection, GeoJSON } from 'geojson';
	import maplibregl, {
		AttributionControl,
		GeolocateControl,
		LngLat,
		Map,
		NavigationControl,
		ScaleControl
	} from 'maplibre-gl';
	import { getContext, onMount } from 'svelte';

	const apiUrl = dev
		? 'http://localhost:7071/api/'
		: 'https://strava-tools-api.azurewebsites.net/api/';

	let mapStore: MapStore = getContext(MAPSTORE_CONTEXT_KEY);

	let mapContainer: HTMLDivElement;

	let fetchRadius = 20000;
	let cancelFetchZone = 0.5;

	let queriedLocations = new Array<LngLat>();
	let peaks: GeoJSON.FeatureCollection;
	let peakIds = new Set();

	const R = 6371e3; // Earth's radius in meters

	interface SummitedPeak {
		id: string;
		userId: string;
		peakId: string;
		activityIds: string[];
	}

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

	const fetchPeaks = async (center: LngLat): Promise<GeoJSON> => {
		var alreadyFetched = queriedLocations.some((lngLat) => {
			var distance = calculateDistance(lngLat, center);
			return distance < fetchRadius * cancelFetchZone;
		});

		if (alreadyFetched) return peaks;

		queriedLocations.push(center);

		return fetch(`${apiUrl}peaks?lat=${center.lat}&lon=${center.lng}&radius=${fetchRadius}`)
			.then((r) => r.json())
			.then((newPeaks: FeatureCollection) => {
				if (peaks == undefined) {
					peaks = newPeaks;
				} else {
					let filteredPeaks = newPeaks.features.filter((feature) => !peakIds.has(feature.id));
					filteredPeaks.forEach((peak) => peakIds.add(peak.id));
					peaks.features = peaks.features.concat(filteredPeaks);
				}
				return peaks;
			});
	};

	const fetchSummits = async (): Promise<GeoJSON> => {
		return fetch(`${apiUrl}summitedPeaks`, { credentials: 'include' })
			.then((r) => {
				if (r.ok) {
					return r.json();
				}
				if (r.status === 401) {
					$activeSession = false;
				}
				return [];
			})
			.then((summitedPeaks: SummitedPeak[]) => {
				for (var summitedPeak of summitedPeaks) {
					var peakId = summitedPeak.peakId;
					var properties = peaks.features.find((x) => x.id == peakId)?.properties;
					if (properties) properties['summited'] = true;
				}
				return peaks;
			});
	};

	onMount(() => {
		const map = new Map({
			container: mapContainer,
			style: `https://api.maptiler.com/maps/topo-v2/style.json?key=${PUBLIC_MAPTILER_KEY}`,
			center: [13.0509, 63.41698],
			zoom: 12,
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
		map.addControl(new ScaleControl({ maxWidth: 80, unit: 'metric' }), 'bottom-left');
		map.addControl(new AttributionControl({ compact: true }), 'bottom-right');

		mapStore?.set(map);

		map.on('load', () => {
			map.loadImage(peakIcon).then((image) => map.addImage('peakIcon', image.data));
			map.loadImage(summitedPeakIcon).then((image) => map.addImage('summitedPeakIcon', image.data));

			fetchPeaks(map.getCenter()).then((peaks) => {
				map.addSource('places', {
					type: 'geojson',
					data: peaks
				});

				map.addLayer({
					id: 'places',
					type: 'symbol',
					source: 'places',
					layout: {
						'icon-image': ['case', ['has', 'summited'], 'summitedPeakIcon', 'peakIcon'],
						'text-field': [
							'concat',
							['get', 'name'],
							'\n',
							['to-string', ['get', 'elevation']],
							' m'
						],
						'text-font': ['Open Sans Semibold', 'Arial Unicode MS Bold'],
						'text-offset': [0, 1.25],
						'text-anchor': 'top',
						'icon-size': 0.75
					}
				});
			});

			// fetchSummits().then((peaks) => {
			// 	const placesSource = map.getSource('places') as maplibregl.GeoJSONSource;
			// 	placesSource.setData(peaks);
			// });

			map.on('click', 'places', (e) => {
				const description =
					e.features?.at(0)?.properties['elevation'] +
					' groups: ' +
					e.features?.at(0)?.properties['groups'];

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

			map.on('mouseenter', 'places', () => {
				map.getCanvas().style.cursor = 'pointer';
			});

			map.on('mouseleave', 'places', () => {
				map.getCanvas().style.cursor = '';
			});

			map.on('moveend', () => {
				fetchPeaks(map.getCenter()).then((peaks) => {
					const placesSource = map.getSource('places') as maplibregl.GeoJSONSource;
					placesSource.setData(peaks);
				});
			});

			activeSession.subscribe((signedIn) => {
				if (signedIn) {
					fetchSummits().then((peaks) => {
						const placesSource = map.getSource('places') as maplibregl.GeoJSONSource;
						placesSource.setData(peaks);
					});
				}
			});
		});
	});
</script>

<div class="map" data-testid="map" bind:this={mapContainer} />

<style>
	@import 'maplibre-gl/dist/maplibre-gl.css';

	.map {
		position: absolute;
		top: 0;
		bottom: 0;
		width: 100%;
		z-index: 1;
	}
</style>
