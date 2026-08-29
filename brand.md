# Suraksha AR brand

## Brand description

Suraksha AR is an Android safety trainer for newly recruited coal-mine workers. It uses augmented reality to turn safety procedures into short, guided practice sessions that workers can complete offline.

The brand should feel clear, encouraging, practical, and trustworthy. Take inspiration from Duolingo's short lessons, visible progress, friendly feedback, and sense of momentum. Do not copy Duolingo's owl, green palette, illustrations, or interaction patterns.

Suraksha AR should feel like a capable training partner at a mine site. It can be friendly without treating serious safety work like a children's game.

The brand idea is:

> Practise the right action before the real situation happens.

## Product context

The interface is Hindi-first, with Santali in Ol Chiki as a supported language and English as a per-key fallback where needed. The first training modules cover fire and explosion response, gas leaks, and confined-space procedures.

Training works offline because mine sites may have unreliable network access. Each training module is a versioned scenario bundle containing the procedure, scoring rules, training cues, and AR scene references.

The app records a worker's training attempt and the ordered actions taken during it. A passing attempt may later produce a training-completion certificate after an authorized organization validates it. The app must not claim statutory recognition or replace approved mine safety procedures.

## Brand voice

Suraksha AR uses short, direct sentences. It gives one instruction at a time and tells the worker what to do next.

The voice is:

- Clear, because instructions may be read under pressure.
- Respectful, because the worker is learning a real skill.
- Encouraging, without sounding childish.
- Calm when the worker makes a mistake.
- Firm when an action is unsafe.
- Practical and free of corporate language.

Use wording such as:

- "Choose the correct extinguisher."
- "Move to the safe zone."
- "Good. The hazard was identified."
- "That action is unsafe. Try the previous step."
- "Walk to the marked exit."

Avoid wording such as:

- "Oopsie!"
- "You failed!"
- "Amazing work, superstar!"
- "Level up your safety game!"

Encouragement can be warm. Safety instructions must stay exact.

## Product introduction

Suraksha AR helps mine workers practise emergency procedures using an Android phone. Each training module presents a realistic situation, gives the worker a clear next action, and checks whether they performed it in the correct order.

The app helps workers build familiarity before they face a real emergency. It does not replace site training, supervisors, approved equipment, or emergency instructions.

## Visual direction

The visual style combines the progress clarity of a learning app with the discipline of industrial safety equipment and the spatial cues of augmented reality.

Use rounded cards, large action buttons, simple illustrations, clear progress indicators, and strong status colors. A worker should understand the current task within two seconds.

The interface should feel calm and focused. Avoid visual clutter, excessive decoration, and childish game language.

## Color system

Use a dark coal-inspired base with high-visibility safety colors.

| Name | Hex | Use |
| --- | --- | --- |
| Coal black | `#15191C` | Main background and deep surfaces |
| Graphite | `#252B30` | Cards, panels, and secondary surfaces |
| Warm ash | `#F1EEE7` | Main text on dark surfaces |
| Safety yellow | `#F4C542` | Primary actions and progress |
| Signal orange | `#F47B35` | Active attention and equipment cues |
| Safe green | `#3FA66B` | Confirmed safe states and completed steps |
| Warning red | `#D94A45` | Hazards, critical failures, and blocked actions |
| AR blue | `#4C91D9` | Spatial markers and tracking states |

Do not use color alone to communicate status. Pair color with a label, icon, or short sentence. Check contrast on Android devices, including Hindi and Ol Chiki text.

## Typography

Use a rounded, highly legible sans-serif family with reliable support for Latin, Devanagari, and Ol Chiki. Noto Sans Devanagari and Noto Sans Ol Chiki are possible starting points, subject to Android testing and final font licensing.

Typography should use:

- Large text for the current instruction.
- Medium-weight labels for module names and progress.
- Regular text for explanations.
- Bold text for actions, warnings, and important status changes.

Allow extra line height for Devanagari and Ol Chiki. Do not force translated text into narrow cards or short buttons.

## Logo direction

The logo should combine the name "Suraksha AR" with a simple mark based on protection and guided movement.

Possible directions include:

- A shield containing an AR targeting frame.
- A mine tunnel leading toward a safe exit.
- A location marker shaped like a protective badge.
- Two offset planes suggesting an AR view and a real-world safety sign.

The mark must work in one color, at small sizes, and without the wordmark. Avoid a generic shield, hard hat, or flame unless the final form is distinctive.

Do not use or imitate Duolingo's owl or wordmark.

## Illustration style

Illustrations should be simple, bold, and readable on a small phone screen.

Use thick rounded outlines, limited colors, clear silhouettes, slight perspective for equipment and mine environments, and restrained character expressions.

Show workers wearing correct protective equipment. Avoid exaggerated danger, panic, or cartoon injuries.

A recurring safety trainer or AR assistant can guide the worker through the app. Keep this character secondary to the instruction. Important information must never depend on speech bubbles or facial expressions alone.

## Interaction principles

### One action at a time

Each screen should present one main instruction. Secondary actions should stay visually quiet.

### Show progress

Show the worker's position in the training attempt with a simple indicator such as `Step 3 of 6`. A connected path or checkpoint system can make progress easy to scan.

Progress should support learning, not competition.

### Teach through mistakes

When a worker performs an action out of order, explain what happened and return them to the required step.

Example:

> This step comes later. First identify the fire type.

Do not shame the worker. Do not hide a critical failure behind a celebration animation.

### Put safety before rewards

A correct safety action should receive clear confirmation. An unsafe action should stop the flow, explain the risk, and make the next safe action obvious.

Do not reward speed over correct procedure.

### Support weak AR tracking

Every AR marker needs a readable label and text instruction. The app must remain understandable when tracking is weak, lighting is poor, or the worker cannot see the marker clearly.

## Training module structure

Training modules should follow a consistent sequence:

1. Brief situation setup.
2. Current hazard or task.
3. One required action.
4. AR interaction or movement check.
5. Immediate feedback.
6. Progress update.
7. Completion or critical-failure state.

Module cards should show the module name, a short description, estimated duration, offline availability, completion status, and a start or resume action.

Use real procedural language in module descriptions. Do not make emergency response sound entertaining.

## Feedback and rewards

Good learning mechanics include step completion, visible progress, resume training, completion history, review sessions, calm positive feedback, and a personal record of completed practice.

Use streaks only if they encourage regular practice without pressuring workers to rush. The strongest reward is confidence that the worker can perform the procedure correctly.

## Localization

The language order is:

1. Hindi.
2. Santali in Ol Chiki.
3. English fallback where explicitly required.

Design with real translated strings early. Do not design only around English text lengths.

Requirements:

- Allow labels to wrap to two or three lines.
- Keep buttons tall enough for larger scripts.
- Avoid placing text inside narrow illustrations.
- Keep icons meaningful without text.
- Preserve the same safety meaning in every language.
- Have safety instructions reviewed by qualified language and domain reviewers.
- Do not use unreviewed machine translation for final Ol Chiki safety wording.

## Motion and sound

Motion should explain state changes, not decorate the interface.

Use a short progress transition after a correct action, a restrained pulse around the next AR target, a clear state change for an invalid action, and a calm completion transition.

Avoid flashing effects, loud celebration sounds, distracting motion, or animations that delay the next instruction.

Sound needs an off switch and must never be the only way to communicate a warning. Use vibration sparingly for confirmations and blocked actions.

## Brand guardrails

Suraksha AR should always be:

- Friendly, but not childish.
- Serious, but not intimidating.
- Localized, not merely translated.
- Clear before decorative.
- Encouraging after correct practice.
- Firm when safety is at risk.
- Designed for offline and outdoor use.
- Distinct from Duolingo in its logo, mascot, colors, illustrations, and wording.

For every screen, the design team should be able to answer this question immediately:

> What does the worker need to do next, and how will the app confirm whether it was safe?
