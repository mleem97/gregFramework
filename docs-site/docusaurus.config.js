// @ts-check

const config = {
  title: 'FrikaModFramework',
  tagline: 'Build Better Mods, Faster.',
  favicon: 'img/logo.svg',
  url: 'https://mleem97.github.io',
  baseUrl: '/FrikaModFramework/',
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
      title: 'FMF.',
      items: [
        {to: '/', label: 'Start', position: 'left'},
        {to: '/docs', label: 'Docs Hub', position: 'left'},
        {to: '/audiences/newbies', label: 'Newbies', position: 'left'},
        {to: '/audiences/intermediates', label: 'Intermediates', position: 'left'},
        {to: '/audiences/professionals', label: 'Pros', position: 'left'},
        {href: 'https://github.com/mleem97/FrikaModFramework/issues', label: 'Support', position: 'right'},
        {href: 'https://github.com/mleem97/FrikaModFramework', label: 'GitHub', position: 'right'},
      ],
    },
  },
};

module.exports = config;
