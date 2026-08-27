# Santali Ol Chiki translation review — questionnaire

**Purpose:** Unblock `https://github.com/aditya-zig/SIH/issues/4`. A Santali-speaking worker who selects Ol Chiki must receive reviewed Ol Chiki text for every required interface key. English stays as a per-key fallback only for one deliberately absent key.

**From:** SIH team, **To:** Santali language reviewer (native speaker literate in Ol Chiki), **How your answers will be used:** Your reviewed strings will replace the empty `mobile/Assets/StreamingAssets/Localization/sat-Olck.json` and be enforced by `JsonLocalizationCatalogTests`.

## Context

Suraksha AR is an Android AR safety trainer for newly recruited coal-mine workers. The interface is Hindi-first, Santali in Ol Chiki second, English fallback. The current `sat-Olck.json` has `reviewStatus: blocked-pending-human-translator` and empty `strings`. The team has no Santali literacy and cannot rely on Google Translate for Ol Chiki. Your review supplies the approved Ol Chiki for each required key and confirms which single key, if any, should stay English as a deliberate fallback. Your approval also changes `reviewStatus` to `reviewed`.

## How to answer

Try to return within 5 days. Roughly 30 minutes. Provide Ol Chiki in Unicode Ol Chiki script, not Latin transliteration. If you are unsure about a term, leave it in English and flag it with a note rather than guessing. Partial answers are useful. Mark anything you want to keep English.

## Required strings — please provide Ol Chiki for each

For each key, write the exact Ol Chiki you approve. These are the 15 required keys from `en.json` and `hi.json`:

### app.title — Suraksha AR
_Current en: Suraksha AR, hi: सुरक्षा एआर_
_Why this matters: App name shown in launcher and header._

>

### action.start — Start training
_Current en: Start training, hi: प्रशिक्षण शुरू करें_

>

### status.offline — Available offline
_Current en: Available offline, hi: ऑफ़लाइन उपलब्ध_

>

### status.complete — Training complete
_Current en: Training complete, hi: प्रशिक्षण पूरा हुआ_

>

### module.fire.title — Fire and explosion response
_Current en: Fire and explosion response, hi: आग और विस्फोट से बचाव_

>

### module.gas.title — Gas leak and confined-space protocol
_Current en: Gas leak and confined-space protocol, hi: गैस रिसाव और सीमित स्थान प्रोटोकॉल_

>

### fire.identify_hazard — Identify the fire before selecting equipment.
_Current en: Identify the fire before selecting equipment., hi: उपकरण चुनने से पहले आग के प्रकार की पहचान करें।_

>

### fire.select_extinguisher — Select the correct extinguisher.
_Current en: Select the correct extinguisher., hi: सही अग्निशामक चुनें।_

>

### fire.remove_pin — Remove the safety pin.
_Current en: Remove the safety pin., hi: सुरक्षा पिन निकालें।_

>

### fire.aim_at_base — Aim at the base of the fire.
_Current en: Aim at the base of the fire., hi: आग के आधार पर निशाना लगाएं।_

>

### fire.discharge — Discharge the extinguisher.
_Current en: Discharge the extinguisher., hi: अग्निशामक चलाएं।_

>

### fire.evacuate — Follow the marked safe exit.
_Current en: Follow the marked safe exit., hi: चिह्नित सुरक्षित निकास का पालन करें।_

>

### gas.recognize_zone — Identify the gas hazard zone.
_Current en: Identify the gas hazard zone., hi: गैस खतरे वाले क्षेत्र की पहचान करें।_

>

### gas.withdraw — Withdraw to the safe zone.
_Current en: Withdraw to the safe zone., hi: सुरक्षित क्षेत्र में पीछे हटें।_

>

### gas.report — Report the hazard to the supervisor.
_Current en: Report the hazard to the supervisor., hi: पर्यवेक्षक को खतरे की सूचना दें।_

>

### gas.select_self_rescuer — Select the approved self-rescuer.
_Current en: Select the approved self-rescuer., hi: स्वीकृत सेल्फ-रेस्क्यूर चुनें।_

>

### gas.buddy_check — Confirm that your buddy is present.
_Current en: Confirm that your buddy is present., hi: पुष्टि करें कि आपका साथी मौजूद है।_

>

### gas.exit — Follow the safe exit sequence.
_Current en: Follow the safe exit sequence., hi: सुरक्षित निकास क्रम का पालन करें।_

>

## Review and fallback decision

### Do you approve machine translation for any of these, or do all need your reviewed Ol Chiki?

>

### Which single key should deliberately stay without a Santali translation so English fallback is tested?

_Why this matters: `JsonLocalizationCatalogTests` proves fallback for one absent key. If you prefer all keys translated, we will keep a separate dummy key for the test._

>

### What review status should we set in `sat-Olck.json` after your approval?

_Options: `reviewed`, `requires-safety-review`, or another status you prefer._

>

## Anything else?

Any cultural or safety wording we should change, or any term that needs a different Ol Chiki spelling for coal-mine workers?

>
