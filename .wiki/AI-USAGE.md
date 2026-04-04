# AI Usage Policy & Disclaimer

- Root policy file: [`AI_POLICY.md`](../AI_POLICY.md)
- Contributor guide: [`CONTRIBUTING.md`](../CONTRIBUTING.md)

## 🇬🇧 English Version

***

### Preamble

We live in a time where Artificial Intelligence has fundamentally changed how software is built, documented, and distributed. Tools like GitHub Copilot, ChatGPT, Claude, Gemini, and countless others have become genuine partners in the development process — capable of generating boilerplate, explaining complex APIs, summarizing large codebases, and accelerating tasks that once took hours into minutes. We recognize and deeply respect this reality.

The **FrikaModFramework** project is a community-driven, open-source modding framework built on top of MelonLoader for Unity-based games. It is, by nature, a large and complex project. The codebase interfaces directly with game assemblies, hooks into runtime internals, and ships code that runs on end-user machines without a sandbox. This makes it a project where **code quality, security, and intentionality are not optional** — they are foundational requirements.

This document represents our full, transparent, and honest position on the use of AI in contributions to this project. It is not written to discourage you. It is written to welcome you — responsibly.

***

### Our Own Use of AI

We want to be fully transparent: **we use AI ourselves.** The maintainers of this project regularly leverage AI-assisted tooling to:

- Extract and process large volumes of data from game assemblies and Unity internals, tasks that would otherwise require days of manual work
- Generate and maintain documentation for APIs, hooks, and the modder-facing interface layer
- Draft boilerplate code structures and scaffolding for new module additions
- Summarize and cross-reference large hook files (such as `assembly-hooks` and `modder-hooks` reference data)
- Assist with commit message formatting, changelog generation, and semantic versioning automation

We do this because it is pragmatic, efficient, and increasingly unavoidable in large-scale open-source work. We are not hypocrites about this. If we ask you to hold yourself to a standard, we hold ourselves to the same standard — and we intend to be open about when and how AI was involved in our own contributions.

***

### AI in Contributions: Our Position

We **do not discourage** the use of AI in your contributions to FrikaModFramework. Quite the opposite — we recognize that AI can help you write better documentation, understand unfamiliar APIs, produce consistent code style, or generate test cases you might not have thought of. These are genuinely valuable capabilities and we welcome them.

However, **using AI responsibly in an open-source context is your responsibility as a contributor.** AI models do not run your code against the actual game. They do not know the specific quirks of MelonLoader's harmony patching system, the undocumented behaviors in the game's IL code, or the ways in which a poorly patched method can corrupt save data, crash the game, or — in worst cases — introduce exploitable behavior. **You do.**

Therefore, we ask the following of every contributor who uses AI in any capacity:

**Test everything.** Run your code. Run it again. Run it in edge cases. Don't trust that the AI's output compiles clean as proof that it works. A C# snippet that compiles is not a safe mod hook. Test against the actual game runtime. Test with MelonLoader's latest and recent prior versions. Document what you tested and how.

**Audit for security.** Game mods that run on end-user machines carry a unique trust burden. Your mod may be installed by hundreds or thousands of players who have no way to audit your code themselves. If your AI-generated code includes file I/O, network calls, process spawning, registry access, or reflection-based invocation, you are responsible for ensuring none of it does anything unintended. If you are unsure, ask. Open an issue. Start a discussion. We would rather review a question than merge a vulnerability.

**Understand what you submit.** If you cannot explain line-by-line what a section of your submitted code does, it should not be in a pull request. This is not about punishing you for using AI — it is about ensuring that every line that enters this codebase was understood and validated by a human before it was merged. AI can help you write it. AI cannot replace you understanding it.

**Disclose when AI was significantly involved.** We are not asking you to write an essay in every PR. A simple note — "generated with Copilot and reviewed/tested manually" or "documentation drafted with ChatGPT, verified against actual API" — is enough. Transparency builds trust. It helps reviewers know where to focus their attention and helps the community understand the workflow that produced a contribution.

***

### On AI-Generated Content Presented as Human Work

We have a firm but fair policy on this: **if you present obviously AI-generated content as entirely your own original work and this is clearly not the case, we reserve the right to reject the contribution and, depending on the severity and repetition, to exclude you from the project.**

This is not about gatekeeping creativity or punishing tool use. It is about honesty. The open-source community runs on trust. When you contribute to a project, you are implicitly telling maintainers and the community: "I stand behind this work." That statement means something. It carries weight. If a reviewer spends significant time reviewing a PR only to determine that the submitted code is an unmodified, untested AI output — complete with hallucinated API calls to methods that don't exist, stubs that were never implemented, or documentation that describes functionality the code doesn't have — that is a waste of everyone's time and, depending on the code's nature, potentially dangerous.

We are not infallible detectors of AI-generated code. We are not running GPT-zero on every commit. But the modding and open-source communities have developed a collective sense for certain patterns: unusually generic variable naming in otherwise-specific contexts, documentation that describes plausible-but-incorrect behavior, code that is syntactically perfect but logically inconsistent with the surrounding codebase, and explanations that collapse under simple follow-up questions. If a PR exhibits these patterns and the contributor insists the work is entirely their own, that is where trust breaks down.

We extend good faith to everyone. We ask for good faith in return.

***

### Security is Not Optional

Because this framework operates at a low level — patching IL code, hooking into the Unity runtime, and injecting into a live game process — **every contribution to the core framework must be treated as potentially security-critical.** This is not an exaggeration. A malicious or negligent patch to a harmony hook can:

- Expose file system paths or system information unintentionally
- Enable or facilitate remote code execution if combined with other mod behavior
- Corrupt game data in ways that propagate to cloud saves
- Create instability that is difficult to attribute and hard to reproduce

AI models are not security auditors. They are trained on vast amounts of code, much of which contains vulnerabilities. They can and do generate code that looks correct, feels idiomatic, and still contains subtle security issues that only become visible at runtime under specific conditions. **You are the security auditor of your own contributions.** We will also review with security in mind, but the first line of defense is you.

If you are unsure whether something you are contributing could have security implications, please open a draft PR or start a discussion before submitting. Security-conscious contributors are among the most valuable members of any open-source community, and we treat them accordingly.

***

### Who We Welcome

Everyone. Seriously. Whether you are a seasoned C# developer, a game modder who learned to code by reading other mods, someone who is learning their first programming language and using AI as a tutor, or a documentation writer who has never touched game code — if you want to help, there is a place for you here.

The standards described above are not entry barriers. They are the framework within which collaboration works. They exist to protect end users, to protect contributors from one another, and to protect the project from entropy. They apply equally to the maintainers as they do to first-time contributors.

We believe that the best mods — like the best software — are built by communities where people are honest with each other, where quality is a shared value rather than an individual burden, and where the tools we use (including AI) serve the humans doing the work rather than replacing the judgment those humans are responsible for.

Welcome to FrikaModFramework. We are glad you are here.

***

### Quick Reference: AI Contribution Checklist

Before submitting any PR where AI tooling was involved to any significant degree, work through this checklist honestly:

- [ ] I have run and tested this code against the actual game runtime
- [ ] I understand what every section of my submitted code does
- [ ] I have reviewed for unintended file I/O, network calls, or reflection-based access
- [ ] I have noted in the PR description where AI tooling was significantly involved
- [ ] I have verified that documentation matches the actual behavior of the code
- [ ] I have tested against the latest stable version of MelonLoader
- [ ] I am prepared to discuss and defend any part of my submission in review

***
***

## 🇩🇪 Deutsche Version

***

### Präambel

Wir leben in einer Zeit, in der Künstliche Intelligenz grundlegend verändert hat, wie Software entwickelt, dokumentiert und verteilt wird. Werkzeuge wie GitHub Copilot, ChatGPT, Claude, Gemini und unzählige andere sind zu echten Partnern im Entwicklungsprozess geworden — fähig, Boilerplate zu generieren, komplexe APIs zu erklären, große Codebasen zusammenzufassen und Aufgaben, die früher Stunden gedauert haben, in Minuten zu erledigen. Diese Realität erkennen wir an und respektieren sie aufrichtig.

Das **FrikaModFramework**-Projekt ist ein community-getriebenes, quelloffenes Modding-Framework, das auf MelonLoader für Unity-basierte Spiele aufbaut. Es ist von Natur aus ein großes und komplexes Projekt. Die Codebasis interagiert direkt mit Game-Assemblies, hakt sich in Laufzeit-Interna ein und liefert Code aus, der auf den Maschinen der Endnutzer läuft — ohne Sandbox. Das macht es zu einem Projekt, bei dem **Codequalität, Sicherheit und Absichtlichkeit keine Optionen sind** — sie sind grundlegende Anforderungen.

Dieses Dokument stellt unsere vollständige, transparente und ehrliche Position zur Nutzung von KI bei Beiträgen zu diesem Projekt dar. Es wurde nicht geschrieben, um dich zu entmutigen. Es wurde geschrieben, um dich zu begrüßen — verantwortungsvoll.

***

### Unsere eigene Nutzung von KI

Wir möchten vollständig transparent sein: **Wir selbst nutzen KI.** Die Maintainer dieses Projekts greifen regelmäßig auf KI-gestützte Werkzeuge zurück, um:

- Große Datenmengen aus Game-Assemblies und Unity-Interna zu extrahieren und zu verarbeiten — Aufgaben, die andernfalls tagelange manuelle Arbeit erfordern würden
- Dokumentation für APIs, Hooks und die modder-seitige Schnittstellenschicht zu erstellen und zu pflegen
- Boilerplate-Codestrukturen und Gerüste für neue Modulerweiterungen zu entwerfen
- Große Hook-Dateien (wie die `assembly-hooks`- und `modder-hooks`-Referenzdaten) zusammenzufassen und querzuverweisen
- Bei der Formatierung von Commit-Nachrichten, der Changelog-Generierung und der Automatisierung von Semantic Versioning zu helfen

Wir tun dies, weil es pragmatisch, effizient und in großangelegter Open-Source-Arbeit zunehmend unvermeidbar ist. Wir sind in dieser Hinsicht keine Heuchler. Wenn wir von euch einen bestimmten Standard verlangen, halten wir uns selbst an denselben Standard — und wir beabsichtigen, offen darüber zu sein, wann und wie KI in unsere eigenen Beiträge eingeflossen ist.

***

### KI in Contributions: Unsere Position

Wir **raten nicht von der Nutzung von KI** bei deinen Beiträgen zu FrikaModFramework ab. Ganz im Gegenteil — wir erkennen an, dass KI dir dabei helfen kann, bessere Dokumentation zu schreiben, unbekannte APIs zu verstehen, einen konsistenten Codestil zu entwickeln oder Testfälle zu generieren, auf die du sonst vielleicht nicht gekommen wärst. Das sind echte Mehrwerte, die wir begrüßen.

**Der verantwortungsvolle Einsatz von KI in einem Open-Source-Kontext liegt jedoch in deiner Verantwortung als Contributor.** KI-Modelle führen deinen Code nicht gegen das echte Spiel aus. Sie kennen die spezifischen Eigenheiten von MelonLoaders Harmony-Patching-System nicht, die undokumentierten Verhaltensweisen im IL-Code des Spiels oder die Arten, in denen ein schlecht gepatchter Methodenaufruf Speicherdaten beschädigen, das Spiel zum Absturz bringen oder — im schlimmsten Fall — ausnutzbare Verhaltensweisen einführen kann. **Du kennst sie.**

Daher bitten wir jeden Contributor, der KI in irgendeiner Kapazität nutzt, Folgendes zu beachten:

**Teste alles.** Führe deinen Code aus. Führe ihn erneut aus. Führe ihn in Edge Cases aus. Vertrau nicht darauf, dass ein sauber kompilierender KI-Output ein Beweis dafür ist, dass er funktioniert. Ein C#-Snippet, das kompiliert, ist kein sicherer Mod-Hook. Teste gegen die echte Game-Runtime. Teste mit der aktuellen und kürzlich vorherigen Version von MelonLoader. Dokumentiere, was du getestet hast und wie.

**Prüfe auf Sicherheitslücken.** Game-Mods, die auf Endnutzer-Maschinen laufen, tragen eine einzigartige Vertrauenslast. Dein Mod wird möglicherweise von Hunderten oder Tausenden von Spielern installiert, die keine Möglichkeit haben, deinen Code selbst zu prüfen. Wenn dein KI-generierter Code Datei-I/O, Netzwerkaufrufe, Prozessinstanziierung, Registry-Zugriffe oder Reflection-basierte Aufrufe enthält, bist du dafür verantwortlich, sicherzustellen, dass nichts davon unbeabsichtigte Dinge tut. Wenn du dir nicht sicher bist, frag nach. Erstelle ein Issue. Starte eine Diskussion. Wir überprüfen lieber eine Frage als eine Sicherheitslücke zu mergen.

**Verstehe, was du einreichst.** Wenn du nicht Zeile für Zeile erklären kannst, was ein Abschnitt deines eingereichten Codes macht, sollte er nicht in einem Pull Request landen. Es geht nicht darum, dich für die Nutzung von KI zu bestrafen — es geht darum sicherzustellen, dass jede Zeile, die in diese Codebasis eingeht, von einem Menschen verstanden und validiert wurde, bevor sie gemergt wurde. KI kann dir helfen, sie zu schreiben. KI kann nicht ersetzen, dass du sie verstehst.

**Offenbare, wenn KI erheblich beteiligt war.** Wir bitten dich nicht, in jedem PR einen Essay zu schreiben. Ein einfacher Hinweis — "mit Copilot generiert und manuell überprüft/getestet" oder "Dokumentation mit ChatGPT entworfen, gegen die tatsächliche API verifiziert" — reicht aus. Transparenz schafft Vertrauen. Sie hilft Reviewern zu wissen, worauf sie ihre Aufmerksamkeit richten sollen, und hilft der Community, den Workflow zu verstehen, der einen Beitrag hervorgebracht hat.

***

### Zu KI-generiertem Inhalt, der als eigene Arbeit präsentiert wird

Dazu haben wir eine klare, aber faire Position: **Wenn du offensichtlich KI-generierten Inhalt als vollständig eigene, originäre Arbeit präsentierst und dies eindeutig nicht der Fall ist, behalten wir uns das Recht vor, den Beitrag abzulehnen und — abhängig von Schwere und Wiederholung — dich vom Projekt auszuschließen.**

Es geht nicht darum, Kreativität zu regulieren oder Toolnutzung zu bestrafen. Es geht um Ehrlichkeit. Die Open-Source-Community lebt von Vertrauen. Wenn du zu einem Projekt beiträgst, sagst du Maintainern und der Community implizit: „Ich stehe hinter dieser Arbeit." Diese Aussage bedeutet etwas. Sie hat Gewicht. Wenn ein Reviewer erhebliche Zeit damit verbringt, einen PR zu prüfen, nur um festzustellen, dass der eingereichte Code ein unveränderter, ungetesteter KI-Output ist — komplett mit halluzinierten API-Aufrufen zu Methoden, die nicht existieren, Stubs, die nie implementiert wurden, oder Dokumentation, die Funktionalität beschreibt, die der Code nicht hat — ist das eine Verschwendung der Zeit aller Beteiligten und je nach Art des Codes potenziell gefährlich.

Wir sind keine unfehlbaren Detektoren für KI-generierten Code. Wir führen kein GPT-Zero über jeden Commit aus. Aber die Modding- und Open-Source-Communities haben ein kollektives Gespür für bestimmte Muster entwickelt: ungewöhnlich generische Variablenbenennung in ansonsten spezifischen Kontexten, Dokumentation, die plausibles, aber falsches Verhalten beschreibt, Code, der syntaktisch perfekt aber logisch inkonsistent mit der umgebenden Codebasis ist, und Erklärungen, die bei einfachen Nachfragen in sich zusammenfallen. Wenn ein PR diese Muster aufweist und der Contributor darauf besteht, dass die Arbeit vollständig seine eigene ist, dann bricht dort das Vertrauen zusammen.

Wir schenken jedem Vertrauen. Wir bitten darum, dass dieses Vertrauen erwidert wird.

***

### Sicherheit ist nicht optional

Da dieses Framework auf einer niedrigen Ebene arbeitet — IL-Code patcht, sich in die Unity-Runtime einhängt und in einen laufenden Spielprozess injiziert — **muss jeder Beitrag zum Core-Framework als potenziell sicherheitskritisch behandelt werden.** Das ist keine Übertreibung. Ein böswilliger oder fahrlässiger Patch eines Harmony-Hooks kann:

- Unbeabsichtigt Dateisystempfade oder Systeminformationen preisgeben
- Remote Code Execution ermöglichen oder erleichtern, wenn er mit anderem Mod-Verhalten kombiniert wird
- Spieldaten auf eine Weise beschädigen, die sich auf Cloud-Saves überträgt
- Instabilität erzeugen, die schwer zuzuordnen und schwer zu reproduzieren ist

KI-Modelle sind keine Sicherheitsauditoren. Sie werden auf riesigen Mengen an Code trainiert, von dem ein großer Teil Sicherheitslücken enthält. Sie können und erzeugen Code, der korrekt aussieht, idiomatisch wirkt und dennoch subtile Sicherheitsprobleme enthält, die erst zur Laufzeit unter bestimmten Bedingungen sichtbar werden. **Du bist der Sicherheitsauditor deiner eigenen Beiträge.** Wir werden ebenfalls mit Blick auf Sicherheit reviewen, aber die erste Verteidigungslinie bist du.

Wenn du dir nicht sicher bist, ob etwas, das du einreichst, Sicherheitsimplikationen haben könnte, erstelle bitte einen Draft-PR oder starte eine Diskussion, bevor du einreichst. Sicherheitsbewusste Contributors gehören zu den wertvollsten Mitgliedern jeder Open-Source-Community, und wir behandeln sie entsprechend.

***

### Wen wir begrüßen

Jeden. Ernsthaft. Ob du ein erfahrener C#-Entwickler bist, ein Game-Modder, der das Programmieren durch das Lesen anderer Mods gelernt hat, jemand, der seine erste Programmiersprache lernt und KI als Tutor nutzt, oder ein Dokumentationsautor, der noch nie Game-Code angefasst hat — wenn du helfen möchtest, gibt es hier einen Platz für dich.

Die oben beschriebenen Standards sind keine Eintrittshürden. Sie sind der Rahmen, innerhalb dessen Zusammenarbeit funktioniert. Sie existieren, um Endnutzer zu schützen, Contributors voreinander zu schützen und das Projekt vor Entropie zu bewahren. Sie gelten für Maintainer genauso wie für erstmalige Contributors.

Wir glauben, dass die besten Mods — wie die beste Software — von Communities gebaut werden, in denen Menschen ehrlich miteinander sind, in denen Qualität ein gemeinsamer Wert und keine individuelle Last ist, und in denen die Werkzeuge, die wir nutzen (einschließlich KI), den Menschen dienen, die die Arbeit leisten, anstatt das Urteilsvermögen zu ersetzen, für das diese Menschen verantwortlich sind.

Willkommen bei FrikaModFramework. Wir freuen uns, dass du hier bist.

***

### Kurzreferenz: KI-Contribution-Checkliste

Bevor du einen PR einreichst, bei dem KI-Tools in erheblichem Umfang beteiligt waren, geh diese Checkliste ehrlich durch:

- [ ] Ich habe diesen Code gegen die echte Game-Runtime ausgeführt und getestet
- [ ] Ich verstehe, was jeder Abschnitt meines eingereichten Codes macht
- [ ] Ich habe auf unbeabsichtigten Datei-I/O, Netzwerkaufrufe oder Reflection-basierten Zugriff geprüft
- [ ] Ich habe in der PR-Beschreibung vermerkt, wo KI-Tooling erheblich beteiligt war
- [ ] Ich habe verifiziert, dass die Dokumentation das tatsächliche Verhalten des Codes widerspiegelt
- [ ] Ich habe gegen die aktuelle stabile Version von MelonLoader getestet
- [ ] Ich bin bereit, jeden Teil meiner Einreichung im Review zu diskutieren und zu verteidigen
