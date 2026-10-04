# Project context (RAG)

Folder for documents that Claude should consult: GDD, Simula briefing, asset notes, decisions.

## Language rule (mandatory)
Everything in the project MUST be written in English: code, comments, commit messages, branch names, UI text, documentation, variable names, logs, error messages — absolutely everything. No exceptions.

Files:
- `gdd.md` — GDD (English translation of the original Portuguese design document; the shipped game diverges where logged in `decisions.md`)
- `architecture.md` — **mandatory** code architecture: state machine with state handlers + dependency injection (composition root)
- `briefing-simula.md` — full take-home briefing text (pending)
- `assets.md` — inventory and sizes of the chosen models (pending)
- `decisions.md` — decisions log (Phase 0 size results, UI approach, tooling); feeds the project note
