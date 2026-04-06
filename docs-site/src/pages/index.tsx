import React, { useMemo } from 'react';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import { motion, type Variants, useReducedMotion } from 'framer-motion';
import { getHomepageContent } from '../i18n/homepage';
import gregImage from '../image.png';
import {
  FaArrowUpRightFromSquare,
  FaBookOpen,
  FaCode,
  FaDiscord,
  FaGithub,
  FaLifeRing,
  FaPeopleGroup,
  FaShop,
  FaScrewdriverWrench,
} from 'react-icons/fa6';

type FeatureItem = {
  icon: React.ReactNode;
  title: string;
  description: string;
};

type DocPathItem = {
  title: string;
  description: string;
  link: string;
};

const viewport = { once: true, margin: '-90px' };

function buildVariants(reducedMotion: boolean) {
  const section: Variants = reducedMotion
    ? { hidden: { opacity: 0 }, show: { opacity: 1 } }
    : {
        hidden: { opacity: 0, y: 26 },
        show: {
          opacity: 1,
          y: 0,
          transition: { duration: 0.65, ease: [0.16, 1, 0.3, 1] },
        },
      };

  const grid: Variants = {
    hidden: {},
    show: {
      transition: {
        staggerChildren: reducedMotion ? 0 : 0.09,
        delayChildren: reducedMotion ? 0 : 0.06,
      },
    },
  };

  const card: Variants = reducedMotion
    ? { hidden: { opacity: 0 }, show: { opacity: 1 } }
    : {
        hidden: { opacity: 0, y: 18, scale: 0.98 },
        show: {
          opacity: 1,
          y: 0,
          scale: 1,
          transition: { duration: 0.45, ease: [0.16, 1, 0.3, 1] },
        },
      };

  const textReveal: Variants = reducedMotion
    ? { hidden: { opacity: 0 }, show: { opacity: 1 } }
    : {
        hidden: { opacity: 0, y: 18 },
        show: {
          opacity: 1,
          y: 0,
          transition: { duration: 0.5, ease: [0.16, 1, 0.3, 1] },
        },
      };

  return { section, grid, card, textReveal };
}

export default function HomePage(): JSX.Element {
  const {
    i18n: { currentLocale },
  } = useDocusaurusContext();

  const t = getHomepageContent(currentLocale);
  const reducedMotion = useReducedMotion();
  const variants = useMemo(() => buildVariants(Boolean(reducedMotion)), [reducedMotion]);

  const features: FeatureItem[] = [
    {
      icon: <FaScrewdriverWrench className="text-xl" />,
      title: t.featureTitles[0],
      description: t.featureDescriptions[0],
    },
    {
      icon: <FaCode className="text-xl" />,
      title: t.featureTitles[1],
      description: t.featureDescriptions[1],
    },
    {
      icon: <FaBookOpen className="text-xl" />,
      title: t.featureTitles[2],
      description: t.featureDescriptions[2],
    },
    {
      icon: <FaPeopleGroup className="text-xl" />,
      title: t.featureTitles[3],
      description: t.featureDescriptions[3],
    },
  ];

  const knowledgePaths: DocPathItem[] = [
    { title: 'Wiki Overview', description: 'Canonical docs entrypoint under /wiki.', link: '/wiki' },
    { title: 'Framework Core', description: 'Runtime hooks, bridge, events and architecture.', link: '/wiki/mods/framework' },
    { title: 'Plugin Wiki', description: 'Plugin-specific docs and release pages.', link: '/wiki/mods/plugins' },
    { title: 'Mod Wiki', description: 'Gameplay mods, release state and module docs.', link: '/wiki/mods/mods' },
    { title: 'FMF Hooks Catalog', description: 'Auto-generated hook strings and event-id map from framework sources.', link: '/wiki/reference/fmf-hooks-catalog' },
    { title: 'Release Channels', description: 'Steam Workshop for discovery; GitHub for beta and alternate DLLs.', link: '/wiki/reference/release-channels' },
    { title: 'Unified Roadmap', description: 'Consolidated roadmap with duplicate tracks removed.', link: '/wiki/roadmap/unified-roadmap' },
    { title: 'Mods Catalog', description: 'Dynamic /mods catalog with wiki and download links.', link: '/mods' },
  ];

  const workflowPaths: DocPathItem[] = [
    { title: 'End-User Docs', description: 'Install, update and troubleshooting paths.', link: '/wiki/wiki-import/EndUser/End-User-Release' },
    { title: 'Mod Developer Docs', description: 'Setup, debug and hook integration guides.', link: '/wiki/wiki-import/ModDevs/Mod-Developer-Debug' },
    { title: 'Repo Inventory', description: 'Monorepo layout, projects, and solution drift for contributors.', link: '/wiki/contributors/repo-inventory' },
    { title: 'Contributor Workflow', description: 'Contribution standards and repository workflow.', link: '/wiki/contributors/docusaurus-workflow' },
    { title: 'Plugin Security Audit', description: 'Git-link submission and malicious-code audit process.', link: '/wiki/contributors/plugin-submission-audit' },
  ];

  return (
    <Layout
      description="Community docs for FrikaMF, standalone Rust stacks, multiplayer, and plugins.">
      <main className="bg-background text-on-surface font-sans min-h-screen editorial-bleed bg-hero-gradient">
        <section className="hero-motion-wrap relative flex min-h-[68vh] flex-col items-center justify-center overflow-hidden px-4 py-20 text-center">
          <div className="hero-particles" aria-hidden="true" />
          <div className="hero-orb hero-orb-pink" aria-hidden="true" />
          <div className="hero-orb hero-orb-green" aria-hidden="true" />

          <motion.h1
            className="homepage-logo-title text-reveal-glow mb-8 text-4xl font-black leading-none tracking-tight text-on-surface md:text-6xl"
            initial="hidden"
            whileInView="show"
            viewport={viewport}
            variants={variants.textReveal}>
            FRIKA MOD <span className="text-primary">🍪</span>
            <br />
            <span className="text-gradient-brand">FRAMEWORK</span>
          </motion.h1>

          <motion.h2
            className="mb-4 max-w-3xl font-headline text-2xl font-bold tracking-tight text-on-surface md:text-4xl"
            initial="hidden"
            whileInView="show"
            viewport={viewport}
            variants={variants.textReveal}
            transition={{ delay: reducedMotion ? 0 : 0.08 }}>
            {t.heroLine1}
            <br />
            <span className="text-on-surface-variant">{t.heroLine2}</span>
          </motion.h2>

          <motion.p
            className="mb-10 max-w-lg text-base font-medium text-on-surface-variant md:text-lg"
            initial="hidden"
            whileInView="show"
            viewport={viewport}
            variants={variants.textReveal}
            transition={{ delay: reducedMotion ? 0 : 0.14 }}>
            {t.heroSub1}
            <br />
            {t.heroSub2}
          </motion.p>

          <motion.div
            className="flex flex-wrap items-center justify-center gap-3"
            initial="hidden"
            whileInView="show"
            viewport={viewport}
            variants={variants.textReveal}
            transition={{ delay: reducedMotion ? 0 : 0.22 }}>
            <Link to="/wiki/mods/framework" className="btn-primary hero-glow px-8 py-4 rounded-xl text-lg">
              {t.ctaStart}
            </Link>
            <Link to="/mods" className="btn-outline px-8 py-4 rounded-xl text-lg">
              {t.ctaMods}
            </Link>
          </motion.div>
        </section>

        <motion.section
          id="features"
          className="section-surface-alt px-4 py-20"
          initial="hidden"
          whileInView="show"
          viewport={viewport}
          variants={variants.section}>
          <motion.div className="mx-auto grid max-w-6xl grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4" variants={variants.grid}>
            {features.map((feature) => (
              <motion.article
                key={feature.title}
                className="app-card app-card-motion app-card-glow glass-card p-5 rounded-xl text-on-surface"
                variants={variants.card}
                whileHover={
                  reducedMotion
                    ? undefined
                    : {
                        y: -6,
                        rotateX: 2,
                        rotateY: -2,
                        scale: 1.01,
                        transition: { type: 'spring', stiffness: 280, damping: 18 },
                      }
                }
                style={{ transformStyle: 'preserve-3d' }}>
                <h3 className="mb-2 flex items-center gap-2 font-headline text-lg font-bold text-on-surface">
                  <span className="text-primary">{feature.icon}</span>
                  <span>{feature.title}</span>
                </h3>
                <p className="text-sm font-medium text-on-surface-variant">{feature.description}</p>
              </motion.article>
            ))}
          </motion.div>
        </motion.section>

        <motion.section
          id="docs"
          className="px-4 py-20"
          initial="hidden"
          whileInView="show"
          viewport={viewport}
          variants={variants.section}>
          <div className="mx-auto max-w-5xl text-center">
            <motion.h2 className="mb-10 font-headline text-3xl font-bold text-on-surface" variants={variants.textReveal}>
              Knowledge Architecture
            </motion.h2>
            <motion.div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3" variants={variants.grid}>
              {knowledgePaths.map((doc) => (
                <motion.div key={doc.link} variants={variants.card}>
                  <Link to={doc.link} className="app-card app-card-motion app-card-glow rounded-lg p-5 text-left text-on-surface block group">
                    <div className="mb-2 font-headline text-lg font-bold text-on-surface transition-colors group-hover:text-primary">{doc.title}</div>
                    <div className="text-sm text-on-surface-variant">{doc.description}</div>
                  </Link>
                </motion.div>
              ))}
            </motion.div>
          </div>
        </motion.section>

        <motion.section
          id="ecosystem"
          className="section-surface-alt px-4 py-20"
          initial="hidden"
          whileInView="show"
          viewport={viewport}
          variants={variants.section}>
          <div className="mx-auto max-w-5xl text-center">
            <motion.h2 className="mb-10 font-headline text-3xl font-bold text-on-surface" variants={variants.textReveal}>
              Workflows
            </motion.h2>
            <motion.div className="grid grid-cols-1 gap-4 md:grid-cols-2" variants={variants.grid}>
              {workflowPaths.map((doc) => (
                <motion.div key={doc.link} variants={variants.card}>
                  <Link to={doc.link} className="app-card app-card-motion app-card-glow rounded-lg p-5 text-left text-on-surface block group">
                    <div className="mb-2 font-headline text-lg font-bold text-on-surface transition-colors group-hover:text-primary">{doc.title}</div>
                    <div className="text-sm text-on-surface-variant">{doc.description}</div>
                  </Link>
                </motion.div>
              ))}
            </motion.div>
          </div>
        </motion.section>

        <motion.section
          id="greg-story"
          className="px-4 py-20"
          initial="hidden"
          whileInView="show"
          viewport={viewport}
          variants={variants.section}>
          <div className="mx-auto max-w-6xl">
            <motion.div className="app-card app-card-glow rounded-2xl p-6 md:p-8 flex flex-col md:flex-row md:items-center md:justify-between gap-6" variants={variants.card}>
              <div>
                <h2 className="mb-4 font-headline text-2xl font-bold text-on-surface md:text-3xl">{t.gregTitle}</h2>
                <p className="mb-4 text-on-surface-variant text-sm md:text-base leading-relaxed max-w-md">
                  {t.gregText1}
                </p>
                <p className="mb-4 text-on-surface-variant text-sm md:text-base leading-relaxed max-w-md">
                  {t.gregText2}
                </p>
                <p className="font-headline text-lg font-bold text-secondary italic">{t.gregQuote}</p>
              </div>
              <motion.div className="shrink-0" whileHover={reducedMotion ? undefined : { rotate: 1.2, y: -3 }}>
                <div className="w-32 h-40 md:w-48 md:h-56 overflow-hidden rounded-xl border-2 border-primary/25">
                  <img src={gregImage} alt="Greg" className="h-full w-full object-cover" />
                </div>
              </motion.div>
            </motion.div>
          </div>
        </motion.section>

        <motion.section
          id="community"
          className="px-4 py-16"
          initial="hidden"
          whileInView="show"
          viewport={viewport}
          variants={variants.section}>
          <div className="mx-auto max-w-6xl">
            <motion.div
              className="mb-6 rounded-xl border border-outline-variant/15 bg-surface-container-high p-4"
              variants={variants.card}>
              <div className="text-sm font-semibold uppercase tracking-wide text-tertiary">{t.comingSoon}</div>
              <div className="mt-1 text-base font-medium text-on-surface">{t.comingSoonText}</div>
            </motion.div>

            <motion.div className="app-card app-card-glow p-6 rounded-xl flex flex-col md:flex-row md:items-center md:justify-between gap-6" variants={variants.card}>
              <div>
                <h3 className="font-headline text-2xl font-bold text-on-surface">{t.communityTitle}</h3>
                <p className="mt-2 text-on-surface-variant">{t.communityText}</p>
              </div>
              <div className="flex flex-wrap gap-3">
                <Link to="https://frikadellental.de" className="btn-social">
                  <FaArrowUpRightFromSquare /> frikadellental.de
                </Link>
                <Link to="/mods" className="btn-social">
                  <FaShop /> {t.availableModsLabel}
                </Link>
                <Link to="https://github.com/mleem97/gregFramework" className="btn-social">
                  <FaGithub /> {t.repositoryLabel}
                </Link>
                <Link to="https://discord.gg/greg" className="btn-social bg-[#5865F2] border-transparent text-white hover:bg-[#4752C4]">
                  <FaDiscord /> {t.joinLabel}
                </Link>
              </div>
            </motion.div>
          </div>
        </motion.section>

        <motion.section
          id="support"
          className="section-surface-alt px-4 py-16"
          initial="hidden"
          whileInView="show"
          viewport={viewport}
          variants={variants.section}>
          <div className="mx-auto max-w-6xl flex flex-col md:flex-row md:items-center md:justify-between gap-4">
            <div>
              <h3 className="font-headline text-2xl font-bold text-on-surface">{t.supportTitle}</h3>
              <p className="text-on-surface-variant">{t.supportText}</p>
            </div>
            <motion.div whileHover={reducedMotion ? undefined : { y: -2, scale: 1.01 }}>
              <Link
                to="https://github.com/mleem97/gregFramework/issues"
                className="btn-primary inline-flex items-center gap-2 rounded-xl px-5 py-3">
                <FaLifeRing /> {t.supportCta}
              </Link>
            </motion.div>
          </div>
        </motion.section>
      </main>

    </Layout>
  );
}

