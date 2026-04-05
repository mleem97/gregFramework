// @ts-check

const config = {
  title: 'Frika Mod Framework',
  tagline: 'Community-driven docs for FrikaMF, plugins, Rust FFI, and multiplayer stacks',
  favicon: 'img/logo.svg',
  url: 'https://frikadellental.de',
  baseUrl: '/',
  onBrokenLinks: 'warn',
  markdown: {
    hooks: {
      onBrokenMarkdownLinks: 'warn',
    },
  },
  scripts: ['/js/auto-locale.js', '/js/canonical-host.js'],
  i18n: {
    defaultLocale: 'en',
    locales: ['en', 'de', 'fr', 'es', 'ru', 'ja'],
    localeConfigs: {
      en: {label: 'English', htmlLang: 'en-GB'},
      de: {label: 'Deutsch', htmlLang: 'de-DE'},
      fr: {label: 'Français', htmlLang: 'fr-FR'},
      es: {label: 'Español', htmlLang: 'es-ES'},
      ru: {label: 'Русский', htmlLang: 'ru-RU'},
      ja: {label: '日本語', htmlLang: 'ja-JP'},
    },
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
  themes: ['@docusaurus/theme-live-codeblock'],
  plugins: [
    '@docusaurus/plugin-css-cascade-layers',
    [
      '@docusaurus/plugin-client-redirects',
      {
        redirects: [
          {
            to: '/mods/standalone',
            from: ['/mods', '/plugins', '/standalone-mods'],
          },
          {
            to: '/mods/framework',
            from: ['/framework'],
          },
        ],
      },
    ],
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
      title: 'Frika Mod Framework',
      hideOnScroll: false,
      style: 'dark',
      items: [
        {to: '/', label: 'Home', position: 'left'},
        {
          label: 'Docs Hub',
          position: 'left',
          items: [
            {to: '/docs', label: 'Overview'},
            {to: '/mods/framework', label: 'Framework'},
            {to: '/mods/standalone', label: 'Standalone Mods'},
            {to: '/wiki-import/Framework-Features-Use-Cases', label: 'Ecosystem'},
          ],
        },
        {
          type: 'localeDropdown',
          className: 'nav-locale nav-right-icon',
          position: 'right',
        },
        {to: '/mods/standalone', label: 'Mods', position: 'right', className: 'nav-right-icon nav-link-mods'},
        {href: 'https://discord.gg/greg', label: 'Discord', position: 'right', className: 'nav-right-icon nav-link-discord'},
        {href: 'https://github.com/mleem97/FrikaModFramework/issues', label: 'Support', position: 'right', className: 'nav-right-icon nav-link-support'},
        {href: 'https://github.com/mleem97/FrikaModFramework', label: 'GitHub', position: 'right', className: 'nav-right-icon nav-link-github'},
      ],
    },
  },
};

module.exports = config;
