/** @type {import('@docusaurus/plugin-content-docs').SidebarsConfig} */
const sidebars = {
  docs: [
    'intro',
    {
      type: 'category',
      label: 'By Experience',
      items: [
        'audiences/newbies',
        'audiences/intermediates',
        'audiences/professionals',
      ],
    },
    {
      type: 'category',
      label: 'Reference',
      items: [
        'reference/wiki-mapping',
        'reference/mod-store-vision',
      ],
    },
    {
      type: 'category',
      label: 'Contributors',
      items: [
        'contributors/docusaurus-workflow',
      ],
    },
    {
      type: 'category',
      label: 'Roadmap',
      items: ['roadmap/mod-store-stages'],
    },
  ],
};

module.exports = sidebars;
