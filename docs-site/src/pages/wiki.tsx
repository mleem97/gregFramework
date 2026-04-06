import React, {useEffect} from 'react';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';

export default function WikiLandingPage(): JSX.Element {
  useEffect(() => {
    if (typeof window !== 'undefined') {
      window.location.replace('/wiki/docs');
    }
  }, []);

  return (
    <Layout title="Wiki" description="Wiki entrypoint">
      <main className="bg-background min-h-screen text-on-surface px-4 py-16">
        <section className="mx-auto max-w-3xl text-center app-card app-card-glow glass-card rounded-xl p-8">
          <h1 className="font-headline text-3xl font-bold text-on-surface mb-3">Wiki</h1>
          <p className="text-on-surface-variant mb-6">Redirecting to the wiki overview...</p>
          <Link to="/wiki/docs" className="button button--primary">
            Open Wiki Overview
          </Link>
        </section>
      </main>
    </Layout>
  );
}
