import ignore from 'rollup-plugin-ignore';
import typescript from '@rollup/plugin-typescript';
import nodeResolve from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';
import terser from '@rollup/plugin-terser';
import versionInjector from 'rollup-plugin-version-injector';

export default [
    {
        external: [],
        input: {
            App: 'src/App.ts',
        },
        logLevel: 'debug',
        output: {
            dir: './../PWP.Maui.Razor/wwwroot/js/App',
            format: 'cjs',
            name: 'Test',
            strict: false,
        },
        plugins: [
            nodeResolve({
                browser: true,
                preferBuiltins: false,
                skip: [],
            }),
            typescript(),
            commonjs(),
            /*ignore(),*/
            terser({
                compress: false,
                format: {
                    beautify: true,
                    comments: false,
                },
                mangle: false,
            }),
            versionInjector({
                injectInTags: {
                    tagId: 'VI',
                    dateFormat: 'yyyy-mm-dd HH:MM:ss',
                },
            }),
        ],
    },
];
/**
 * https://github.com/terser/terser - Terser options
 * https://dev.to/plebras/setting-up-a-javascript-build-process-using-rollup-n1e
 * https://www.npmjs.com/package/rollup-plugin-version-injector - Version Injector documentation
 */