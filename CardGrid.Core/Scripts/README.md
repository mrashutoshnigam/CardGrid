# Card grid client script

`cardgrid.ts` is the source for `wwwroot/js/cardgrid.js` (the jQuery `cardGrid` plugin).
The compiled output is committed; the build does not run TypeScript.

After editing `cardgrid.ts`, regenerate the script from this folder:

```sh
npx -p typescript@5 tsc -p tsconfig.json
```

`types/` holds the jQuery and bootpag declaration files the plugin compiles against.
