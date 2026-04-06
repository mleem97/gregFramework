import React, {useMemo} from 'react';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';
import {moduleCatalog} from '../data/moduleCatalog';

export default function ModsCatalogPage(): JSX.Element {
  const grouped = useMemo(() => {
    const plugins = moduleCatalog.filter((entry) => entry.type === 'plugin');
    const mods = moduleCatalog.filter((entry) => entry.type === 'mod');
    return {plugins, mods};
  }, []);

  return (
    <Layout title="Mods Catalog" description="Dynamic catalog of mods and plugins with wiki and release links.">
      <main className="bg-app-bg min-h-screen text-gray-200 px-4 py-12">
        <section className="mx-auto max-w-6xl mb-10">
          <h1 className="text-4xl font-black text-white mb-3">Mods & Plugins Catalog</h1>
          <p className="text-gray-400">
            This page is generated from the module catalog and links each entry to its wiki page, release page, and
            download route.
          </p>
        </section>

        <section className="mx-auto max-w-6xl mb-10">
          <h2 className="text-2xl font-bold text-white mb-4">Plugins</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {grouped.plugins.map((entry) => (
              <article key={entry.id} className="app-card app-card-motion app-card-glow rounded-xl p-5">
                <h3 className="text-lg font-bold text-white mb-2">{entry.name}</h3>
                <p className="text-sm text-gray-400 mb-4">{entry.description}</p>
                <p className="text-xs text-gray-400 mb-1">Version: {entry.version}</p>
                <p className="text-xs text-gray-400 mb-4">Languages: {entry.languages.join(', ')}</p>
                <div className="flex flex-wrap gap-2">
                  <Link to={entry.wikiPath} className="button button--secondary button--sm">
                    Wiki
                  </Link>
                  <Link to={entry.releasePath} className="button button--secondary button--sm">
                    Release
                  </Link>
                  <a
                    href={entry.downloadPath}
                    className={`button button--sm ${entry.releaseReady ? 'button--primary' : 'button--secondary'}`}
                    aria-disabled={!entry.releaseReady}
                    onClick={(event) => {
                      if (!entry.releaseReady) {
                        event.preventDefault();
                      }
                    }}>
                    {entry.releaseReady ? 'Download DLL' : 'NotReleasedYet'}
                  </a>
                </div>
              </article>
            ))}
          </div>
        </section>

        <section className="mx-auto max-w-6xl">
          <h2 className="text-2xl font-bold text-white mb-4">Mods</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {grouped.mods.map((entry) => (
              <article key={entry.id} className="app-card app-card-motion app-card-glow rounded-xl p-5">
                <h3 className="text-lg font-bold text-white mb-2">{entry.name}</h3>
                <p className="text-sm text-gray-400 mb-4">{entry.description}</p>
                <p className="text-xs text-gray-400 mb-1">Version: {entry.version}</p>
                <p className="text-xs text-gray-400 mb-4">Dependencies: {entry.dependencies.join(', ')}</p>
                <div className="flex flex-wrap gap-2">
                  <Link to={entry.wikiPath} className="button button--secondary button--sm">
                    Wiki
                  </Link>
                  <Link to={entry.releasePath} className="button button--secondary button--sm">
                    Release
                  </Link>
                  <a
                    href={entry.downloadPath}
                    className={`button button--sm ${entry.releaseReady ? 'button--primary' : 'button--secondary'}`}
                    aria-disabled={!entry.releaseReady}
                    onClick={(event) => {
                      if (!entry.releaseReady) {
                        event.preventDefault();
                      }
                    }}>
                    {entry.releaseReady ? 'Download DLL' : 'NotReleasedYet'}
                  </a>
                </div>
              </article>
            ))}
          </div>
        </section>
      </main>
    </Layout>
  );
}
