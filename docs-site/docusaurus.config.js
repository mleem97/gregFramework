// @ts-check

const config = {
  title: 'FrikaModdingFramework Docs',
  tagline: 'Audience-first docs for Newbies, Intermediates, and Pros',
  favicon: 'img/logo.svg',
  url: 'https://example.com',
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
  themeConfig: {
    navbar: {
      title: 'FrikaMF Docs',
      items: [
        {to: '/', label: 'Start', position: 'left'},
        {to: '/audiences/newbies', label: 'Newbies', position: 'left'},
        {to: '/audiences/intermediates', label: 'Intermediates', position: 'left'},
        {to: '/audiences/professionals', label: 'Pros', position: 'left'},
        {href: 'https://github.com/mleem97/FrikaModFramework', label: 'GitHub', position: 'right'},
      ],
    },
  },
};

module.exports = config;
