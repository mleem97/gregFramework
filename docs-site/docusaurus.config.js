// @ts-check

const config = {
  title: 'GregFramework Docs',
  tagline: 'Community-driven docs for frameworks, plugins, and multiplayer stacks',
  favicon: 'img/logo.svg',
  url: 'https://gregframework.eu',
  baseUrl: '/',
  onBrokenLinks: 'warn',
  onBrokenMarkdownLinks: 'warn',
  i18n: {
    defaultLocale: 'en',
    locales: ['en'],
  },
  presets: [
    [
      'classic',
      {
        docs: {
          sidebarPath: require.resolve('./sidebars.js'),
          routeBasePath: '/',
          editUrl: 'https://github.com/mleem97/FrikaModFramework/tree/master/docs-site/',
        },
        blog: false,
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
      },
    ],
  ],
  plugins: [
    function tailwindPlugin() {
      return {
        name: 'tailwindcss-integration',
        /** @param {{plugins: Array<unknown>}} postcssOptions */
        configurePostCss(postcssOptions) {
          postcssOptions.plugins.push(require('@tailwindcss/postcss'));
          return postcssOptions;
        },
      };
    },
  ],
  themeConfig: {
    image: 'img/logo.svg',
    navbar: {
      title: 'GregFramework Docs',
      hideOnScroll: false,
      style: 'dark',
      items: [
        {to: '/', label: 'Start', position: 'left'},
        {to: '/docs', label: 'Docs Hub', position: 'left'},
        {to: '/wiki-import/Framework-Features-Use-Cases', label: 'Ecosystem', position: 'left'},
        {to: '/wiki-import/Framework-Features-Use-Cases', label: 'Framework Core', position: 'left'},
        {to: '/wiki-import/Lua-FFI-Start-Developing', label: 'Rust FFI', position: 'left'},
        {to: '/wiki-import/Steamworks-P2P-Multiplayer-Roadmap', label: 'Multiplayer', position: 'left'},
        {href: 'https://datacentermods.com', label: 'Mod-Store', position: 'right'},
        {href: 'https://github.com/mleem97/FrikaModFramework/issues', label: 'Support', position: 'right'},
        {href: 'https://github.com/mleem97/FrikaModFramework', label: 'GitHub', position: 'right'},
      ],
    },
  },
};

module.exports = config;
