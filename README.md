# Proper Skill Distributor
Proper skill distributor is a relatively simple but useful bannerlord mod mostly made because manually picking the same companion perks and focus points over and over can get exhausting pretty fast. even faster if you're a veteran.
This mod adds preset controls to the character skill screen. You can make reusable skill presets, copy predefined templates, mimic a hero's skillset and have them allocate their perks, focus and attirbute points following those.

## What it does
You can
* manage skill sets for new presets or existing templates
* mimic existing heroes of your clan dynamically
* use a predefined template
* export your own saved templates

## What it does not
* it does not give free points
* it does not manage equipment, troops or party roles
* it is very unlikely to crash your game

## Where it does
Open the character skill screen;
On the top left corner, "Add Presets" opens the preset editor and "Use Presets" lets you assign one of your saved presets to the current hero

## What is what
A preset is a saved skill template that can contain the following;
* attribute targets
* focus targets
* perk choices
The preset is saved into your campaign save.

A predefined template is a character build shipped with the mod
They can be found in: Modules/ProperSkillDistributor/ModuleData/skill_presets.json
There can also be optional template files for certain overhauls like The Old Realms or the ones you can personally export
Templates are not permanent preset slots you can directly use. You have to copy them into one of your normal preset slots
Some templates may include skills or perks from War Sails dlc. if the skill or perk does not exist in your current load order, the mod will ignore that skill or perk

A mimic preset is when you use a hero's skill set as a reference template for others. If you distribute new skill/attribute points or unlock new perks on the referenced hero, other heroes using their template will copy it.

Focus and attribute floor are the baseline values a preset tries to fill first. the floor never goes over the target of that template.

Spend leftover points decides what happens after every template target is reached. if it is on extra points keep going into the highest target assigned skill or attribute that is not maxed yet. if its off the mod stops once the preset target is reached for every skill and attribute.

## How it works
When a hero with an assigned preset gains attribute points, focus points or new perk options the mod checks that preset and tries to move the hero closer to it. 
For attributes and focus points, the mod prioritizes the highest available target in the preset first (by default). Once that target is reached, it moves on to the next highest target.
For focus points, if multiple skills have the same target value, the mod uses their related attribute targets as priority weights.

eg;
template
```text
(a) intelligence = 10
(a) vigor = 7
(f) one-handed = 5
(f) steward = 5
(f) medicine = 4
```

For attribute points, intelligence will be brought up to its preset target (in this case, floor value 10) before the mod starts spending points on vigor
For focus points, steward is prioritized. once it reaches the preset target (in this case, floor value 5), the mod will move on to one handed (next skill with 5 focus points and most related attributes targted), then medicine

UNLESS
Before normal priority spending, the mod can use focus / attribute floor values. their purpose is to bring all of the assigned targets up to the floor value if their template target is over the floor value.

For skills related to more than 1 attribute (eg warsails skills) the mod checks both attributes and uses the higher target attribute value for priority

