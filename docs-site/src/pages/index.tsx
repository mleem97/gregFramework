import React, { useEffect } from 'react';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';
import {
  FaArrowUpRightFromSquare,
  FaBookOpen,
  FaCode,
  FaDiscord,
  FaGithub,
  FaLifeRing,
  FaPeopleGroup,
  FaScrewdriverWrench,
} from 'react-icons/fa6';

type FeatureItem = {
  icon: React.ReactNode;
  title: string;
  description: string;
};

const features: FeatureItem[] = [
  {
    icon: <FaScrewdriverWrench className="text-xl" />,
    title: 'Dual-Track Modding',
    description: 'Build mods in C# directly or native in Rust via C-ABI/FFI.',
  },
  {
    icon: <FaCode className="text-xl" />,
    title: 'Runtime Hook Bridge',
    description: 'Harmony patches, event IDs, and deterministic hook forwarding.',
  },
  {
    icon: <FaBookOpen className="text-xl" />,
    title: 'Wiki-Driven Docs',
    description: 'Source of truth from .wiki with end-user and mod-dev paths.',
  },
  {
    icon: <FaPeopleGroup className="text-xl" />,
    title: 'Community Project',
    description: 'Unofficial and community-driven with open support channels.',
  },
];

function useScrollAnimations() {
  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries, obs) => {
        entries.forEach((entry) => {
          if (!entry.isIntersecting) {
            return;
          }

          entry.target.classList.remove('fade-in-hidden');
          entry.target.classList.add('fade-in-visible');
          obs.unobserve(entry.target);
        });
      },
      { threshold: 0.15 },
    );

    const animated = document.querySelectorAll('.animate-on-scroll');
    animated.forEach((node) => observer.observe(node));

    return () => observer.disconnect();
  }, []);
}

export default function HomePage(): JSX.Element {
  useScrollAnimations();

  return (
    <Layout
      title="FrikaModFramework"
      description="Streamlined. Flexible. Frikatadelle Approved. Build Better Mods, Faster.">
      <header className="border-b border-brandPink-900 bg-brandDark/80 backdrop-blur-md">
        <div className="mx-auto flex w-full max-w-6xl items-center justify-between px-4 py-4">
          <div className="text-xl font-black tracking-tight text-white">FMF.</div>
          <div className="hidden items-center gap-7 text-sm font-semibold text-gray-400 md:flex">
            <a href="#features" className="transition-colors hover:text-brandPink-500">
              FEATURES
            </a>
            <a href="#docs" className="transition-colors hover:text-brandPink-500">
              DOCUMENTATION
            </a>
            <a href="#community" className="transition-colors hover:text-brandPink-500">
              COMMUNITY
            </a>
            <a href="#support" className="transition-colors hover:text-brandPink-500">
              SUPPORT
            </a>
          </div>
          <Link
            to="https://github.com/mleem97/FrikaModFramework"
            className="inline-flex items-center gap-2 rounded border border-gray-700 bg-gray-800 px-4 py-2 text-sm font-bold text-white transition-colors hover:border-gray-500 hover:bg-gray-700">
            <FaGithub className="text-base" />
            GITHUB
          </Link>
        </div>
      </header>

      <main className="bg-brandDark">
        <section className="flex min-h-[70vh] flex-col items-center justify-center bg-linear-to-b from-brandPink-800 to-brandDark px-4 py-24 text-center">
          <div className="mb-8 select-none drop-shadow-2xl fade-in-hidden animate-on-scroll">
            <h1 className="inline-block -rotate-2 rounded-md border-4 border-brandDark bg-gray-100 px-4 py-2 text-5xl font-black text-brandDark md:text-7xl">
              FRIKA MOD <span className="text-brandBrown">🍪</span>
              <br />
              FRAMEWORK
            </h1>
          </div>

          <h2
            className="mb-4 fade-in-hidden animate-on-scroll text-3xl font-extrabold text-white drop-shadow-md md:text-5xl"
            style={{ transitionDelay: '100ms' }}>
            THE DEFINITIVE GAME
            <br />
            MODDING FRAMEWORK.
          </h2>

          <p
            className="mb-8 max-w-2xl fade-in-hidden animate-on-scroll text-lg font-medium text-gray-300 drop-shadow-sm md:text-xl"
            style={{ transitionDelay: '200ms' }}>
            Streamlined. Flexible. Frikatadelle Approved.
            <br />
            Build Better Mods, Faster.
          </p>

          <div className="flex flex-wrap items-center justify-center gap-3">
            <Link
              to="https://github.com/mleem97/FrikaModFramework"
              className="fade-in-hidden animate-on-scroll rounded-full bg-green-600 px-8 py-4 text-lg font-bold text-white shadow-lg shadow-green-900/50 transition-all hover:scale-105 hover:bg-green-500"
              style={{ transitionDelay: '300ms' }}>
              GET STARTED ON GITHUB
            </Link>
            <Link
              to="/docs"
              className="fade-in-hidden animate-on-scroll rounded-full border border-brandPink-900 bg-brandDark px-8 py-4 text-lg font-bold text-white transition-all hover:border-brandPink-500"
              style={{ transitionDelay: '350ms' }}>
              OPEN DOCS HUB
            </Link>
          </div>
        </section>

        <section id="features" className="border-t border-brandPink-900 bg-brandDark px-4 py-20">
          <div className="mx-auto grid max-w-6xl grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-4">
            {features.map((feature, index) => (
              <article
                key={feature.title}
                className="fade-in-hidden animate-on-scroll rounded-xl border border-brandPink-900 bg-gray-900 p-6 text-gray-200 shadow-xl transition-colors hover:border-brandPink-500"
                style={{ transitionDelay: `${index * 100}ms` }}>
                <h3 className="mb-2 flex items-center gap-2 text-xl font-bold text-white">
                  {feature.icon}
                  <span>{feature.title}</span>
                </h3>
                <p className="font-medium text-gray-400">{feature.description}</p>
              </article>
            ))}
          </div>
        </section>

        <section id="docs" className="border-t border-brandPink-800 bg-brandPink-900 px-4 py-20">
          <div className="mx-auto max-w-5xl text-center">
            <h2 className="mb-10 fade-in-hidden animate-on-scroll text-4xl font-bold text-white">Documentation Paths</h2>
            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
              <Link to="/wiki-import/End-User-Release" className="rounded-xl border border-brandPink-800 bg-brandDark/60 p-5 text-left text-gray-200 transition-colors hover:border-brandPink-500">
                <div className="mb-2 text-lg font-bold text-white">End-User</div>
                <div className="text-sm text-gray-300">Install, update, FAQ, troubleshooting.</div>
              </Link>
              <Link to="/wiki-import/Mod-Developer-Debug" className="rounded-xl border border-brandPink-800 bg-brandDark/60 p-5 text-left text-gray-200 transition-colors hover:border-brandPink-500">
                <div className="mb-2 text-lg font-bold text-white">Mod-Devs</div>
                <div className="text-sm text-gray-300">Debug workflows, setup, hooks and references.</div>
              </Link>
              <Link to="/wiki-import/Contirbutors/Contributors-Debug" className="rounded-xl border border-brandPink-800 bg-brandDark/60 p-5 text-left text-gray-200 transition-colors hover:border-brandPink-500">
                <div className="mb-2 text-lg font-bold text-white">Contributors</div>
                <div className="text-sm text-gray-300">Conventions, CI checks, contribution workflow.</div>
              </Link>
              <Link to="/wiki-import/Framework-Features-Use-Cases" className="rounded-xl border border-brandPink-800 bg-brandDark/60 p-5 text-left text-gray-200 transition-colors hover:border-brandPink-500">
                <div className="mb-2 text-lg font-bold text-white">Capability Matrix</div>
                <div className="text-sm text-gray-300">Complete feature map and implementation use cases.</div>
              </Link>
            </div>
          </div>
        </section>

        <section id="community" className="border-t border-brandPink-900 bg-brandDark px-4 py-16">
          <div className="mx-auto flex max-w-6xl flex-col items-start justify-between gap-6 rounded-xl border border-brandPink-900 bg-gray-900 p-6 md:flex-row md:items-center">
            <div>
              <h3 className="text-2xl font-bold text-white">Community & Maintainers</h3>
              <p className="mt-2 text-gray-400">
                FrikaMF is unofficial and community-driven. Follow updates in the repo and join discussions via issues.
              </p>
            </div>
            <div className="flex flex-wrap gap-3">
              <Link to="https://github.com/mleem97/FrikaModFramework" className="inline-flex items-center gap-2 rounded border border-gray-700 bg-gray-800 px-4 py-2 font-bold text-white transition-colors hover:border-gray-500 hover:bg-gray-700">
                <FaGithub />
                Repository
              </Link>
              <Link to="https://github.com/GregTheModder" className="inline-flex items-center gap-2 rounded border border-gray-700 bg-gray-800 px-4 py-2 font-bold text-white transition-colors hover:border-gray-500 hover:bg-gray-700">
                <FaArrowUpRightFromSquare />
                ABOUT ORGA
              </Link>
              <Link to="https://discord.com" className="inline-flex items-center gap-2 rounded bg-[#5865F2] px-4 py-2 font-bold text-white transition-colors hover:bg-[#4752c4]">
                <FaDiscord />
                JOIN
              </Link>
            </div>
          </div>
        </section>

        <section id="support" className="border-t border-brandPink-900 bg-brandDark px-4 py-16">
          <div className="mx-auto flex max-w-6xl flex-col items-start justify-between gap-4 md:flex-row md:items-center">
            <div>
              <h3 className="text-2xl font-bold text-white">Support</h3>
              <p className="text-gray-400">Report bugs, request features, and track current workstreams.</p>
            </div>
            <Link
              to="https://github.com/mleem97/FrikaModFramework/issues"
              className="inline-flex items-center gap-2 rounded border border-brandPink-900 bg-brandPink-800 px-5 py-3 font-bold text-white transition-colors hover:bg-brandPink-500">
              <FaLifeRing />
              Open GitHub Issues
            </Link>
          </div>
        </section>
      </main>

      <footer className="border-t border-brandPink-900 bg-brandDark py-8">
        <div className="mx-auto flex max-w-6xl flex-col items-center justify-between gap-4 px-4 text-sm text-gray-500 md:flex-row">
          <div className="text-2xl font-black text-gray-700">FRIKA MOD 🍪</div>
          <div>
            © 2026{' '}
            <a href="https://github.com/GregTheModder" className="transition-colors hover:text-brandPink-500">
              GregTheModder
            </a>{' '}
            & FrikaModFramework Project. All Rights Reserved.
          </div>
        </div>
      </footer>
    </Layout>
  );
}