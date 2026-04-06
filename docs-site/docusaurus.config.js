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
  scripts: ['/js/canonical-host.js'],
  i18n: {
    defaultLocale: 'en',
    locales: ['en', 'de', 'fr', 'es', 'ru', 'ja'],
    localeConfigs: {
      en: {label: 'English', htmlLang: 'en-GB'},
      de: {label: 'Deutsch', htmlLang: 'de-DE'},
    },
  },
  presets: [
    [
      'classic',
      {
        docs: {
          sidebarPath: require.resolve('./sidebars.js'),
          routeBasePath: '/wiki',
          editUrl: 'https://github.com/mleem97/gregFramework/tree/master/docs-site/',
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
        createRedirects(existingPath) {
          if (existingPath === '/wiki/docs') {
            return ['/docs'];
          }

          if (existingPath.startsWith('/wiki/')) {
            const legacyPath = existingPath.replace('/wiki', '');
            return [legacyPath];
          }

          return undefined;
        },
        redirects: [
          {
            to: '/wiki/mods/framework',
            from: ['/framework'],
          },
          {
            to: '/wiki/mods/plugins/',
            from: ['/plugins', '/standalone-mods'],
          },
          {
            to: 'https://github.com/mleem97/gregFramework/releases/latest/download/FFM.Plugin.AssetExporter.dll',
            from: ['/plugin/FFM.Plugin.AssetExporter.dll'],
          },
          {
            to: 'https://github.com/mleem97/gregFramework/releases/latest/download/FFM.Plugin.Multiplayer.dll',
            from: ['/plugin/FFM.Plugin.Multiplayer.dll'],
          },
          {
            to: 'https://github.com/mleem97/gregFramework/releases/latest/download/FFM.Plugin.PlayerModels.dll',
            from: ['/plugin/FFM.Plugin.PlayerModels.dll'],
          },
          {
            to: 'https://github.com/mleem97/gregFramework/releases/latest/download/FFM.Plugin.Sysadmin.dll',
            from: ['/plugin/FFM.Plugin.Sysadmin.dll'],
          },
          {
            to: 'https://github.com/mleem97/gregFramework/releases/latest/download/FFM.Plugin.WebUIBridge.dll',
            from: ['/plugin/FFM.Plugin.WebUIBridge.dll'],
          },
          {
            to: 'https://github.com/mleem97/gregFramework/releases/latest/download/FMF.ConsoleInputGuard.dll',
            from: ['/mod/FMF.ConsoleInputGuard.dll'],
          },
          {
            to: 'https://github.com/mleem97/gregFramework/releases/latest/download/FMF.GregifyEmployees.dll',
            from: ['/mod/FMF.GregifyEmployees.dll'],
          },
          {
            to: 'https://github.com/mleem97/gregFramework/releases/latest/download/FMF.HexLabelMod.dll',
            from: ['/mod/FMF.HexLabelMod.dll'],
          },
          {
            to: 'https://github.com/mleem97/gregFramework/releases/latest/download/FMF.JoniMLCompatMod.dll',
            from: ['/mod/FMF.JoniMLCompatMod.dll', '/mod/FMF.LangCompatBridge.dll'],
          },
          {
            to: 'https://github.com/mleem97/gregFramework/releases/latest/download/FMF.UIReplacementMod.dll',
            from: ['/mod/FMF.UIReplacementMod.dll'],
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
    colorMode: {
      defaultMode: 'dark',
      disableSwitch: true,
      respectPrefersColorScheme: false,
    },
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
            {to: '/wiki', label: 'Overview'},
            {to: '/wiki/mods/framework', label: 'Framework'},
            {to: '/wiki/mods/plugins', label: 'Plugin Wiki'},
            {to: '/wiki/mods/mods', label: 'Mod Wiki'},
            {to: '/wiki/roadmap/unified-roadmap', label: 'Roadmap'},
          ],
        },
        {
          type: 'localeDropdown',
          className: 'nav-locale nav-right-icon nav-icon-only',
          position: 'right',
          dropdownItemsBefore: [],
          dropdownItemsAfter: [],
        },
        {to: '/mods', label: 'Mods', position: 'right', className: 'nav-right-icon nav-icon-only nav-link-mods', 'aria-label': 'Mods'},
        {href: 'https://discord.gg/greg', label: 'Discord', position: 'right', className: 'nav-right-icon nav-icon-only nav-link-discord', 'aria-label': 'Discord'},
        {href: 'https://github.com/mleem97/gregFramework/issues', label: 'Support', position: 'right', className: 'nav-right-icon nav-icon-only nav-link-support', 'aria-label': 'Support'},
        {href: 'https://github.com/mleem97/gregFramework', label: 'GitHub', position: 'right', className: 'nav-right-icon nav-icon-only nav-link-github', 'aria-label': 'GitHub'},
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Community',
          items: [
            {
              label: 'GitHub',
              href: 'https://github.com/mleem97/gregFramework',
              className: 'footer-link-icon footer-link-github',
            },
            {
              label: 'Discord',
              href: 'https://discord.gg/greg',
              className: 'footer-link-icon footer-link-discord',
            },
            {
              label: 'Support',
              href: 'https://github.com/mleem97/gregFramework/issues',
              className: 'footer-link-icon footer-link-support',
            },
          ],
        },
      ],
      copyright: `Copyright ${new Date().getFullYear()} <a href="https://meyermedia.eu" target="_blank" rel="noopener noreferrer">Meyer Media</a><br/>Dieses Wiki ist ein Community-Projekt und steht in keiner Verbindung zu WASEKU oder dem Spiel selbst.`,
    },
  },
};

module.exports = config;

