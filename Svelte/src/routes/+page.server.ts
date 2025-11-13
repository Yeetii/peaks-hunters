import type { PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ url, cookies }) => {
	const param = url.searchParams.get('pathsBeta');
	if (param) {
		cookies.set('pathsBeta', param, { path: '/', httpOnly: false });
	}
	return {};
};

export const prerender = false;
