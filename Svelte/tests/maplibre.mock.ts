import { vi } from 'vitest';

// Clean mock for maplibre-gl with control DOM elements for Vitest.
vi.mock('maplibre-gl', () => {
	class LngLat {
		constructor(
			public lng: number,
			public lat: number
		) {}
	}
	class NavigationControl {
		onAdd() {
			const root = document.createElement('div');
			root.className = 'maplibregl-ctrl maplibregl-ctrl-navigation';
			['zoom-in', 'zoom-out', 'compass'].forEach((name) => {
				const btn = document.createElement('button');
				btn.className = `maplibregl-ctrl-${name}`;
				root.appendChild(btn);
			});
			return root;
		}
		onRemove() {}
	}
	class GeolocateControl {
		onAdd() {
			const el = document.createElement('button');
			el.className = 'maplibregl-ctrl-geolocate';
			return el;
		}
		onRemove() {}
	}
	class ScaleControl {
		onAdd() {
			const el = document.createElement('div');
			el.className = 'maplibregl-ctrl-scale';
			return el;
		}
		onRemove() {}
	}
	class AttributionControl {
		onAdd() {
			const el = document.createElement('div');
			el.className = 'maplibregl-ctrl-attrib maplibregl-compact';
			el.textContent = '© Test Attribution';
			return el;
		}
		onRemove() {}
	}
	class TerrainControl {
		onAdd() {
			const el = document.createElement('div');
			el.className = 'maplibregl-ctrl-terrain';
			return el;
		}
		onRemove() {}
	}
	class MockMap {
		_container: HTMLElement;
		_style: { layers: unknown[] } = { layers: [] };
		_sources: Record<string, unknown> = {};
		_events: Record<string, ((...args: unknown[]) => void)[]> = {};
		_images: Record<string, unknown> = {};
		constructor(opts: { container: HTMLElement }) {
			this._container = opts.container;
			setTimeout(() => this._emit('load'), 0);
		}
		_emit(evt: string, ...args: unknown[]) {
			(this._events[evt] || []).forEach((fn) => fn(...args));
		}
		on(event: string, cb: (...args: unknown[]) => void) {
			if (!this._events[event]) this._events[event] = [];
			this._events[event].push(cb);
		}
		addControl(control: { onAdd?: (map: MockMap) => HTMLElement }) {
			if (control?.onAdd) {
				const el = control.onAdd(this);
				this._container.appendChild(el);
			}
		}
		addSource(name: string, source: unknown) {
			this._sources[name] = {
				...(source as object),
				setData: (d: unknown) =>
					(this._sources[name] = { ...(this._sources[name] as object), data: d })
			};
		}
		addLayer(layer: unknown) {
			this._style.layers.push(layer);
		}
		loadImage(_img: unknown) {
			return Promise.resolve({ data: '' });
		}
		addImage(name: string, data: unknown) {
			this._images[name] = data;
		}
		getCenter() {
			return { lng: 0, lat: 0, toString: () => '0,0' };
		}
		getZoom() {
			return 12;
		}
		getCanvas() {
			return { style: {} } as HTMLCanvasElement;
		}
		easeTo() {}
		triggerRepaint() {}
		getStyle() {
			return this._style;
		}
		getSource(name: string) {
			return this._sources[name];
		}
	}
	return {
		default: { TerrainControl, Map: MockMap },
		Map: MockMap,
		LngLat,
		NavigationControl,
		GeolocateControl,
		ScaleControl,
		AttributionControl,
		TerrainControl
	};
});
