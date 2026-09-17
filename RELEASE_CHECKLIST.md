# Release checklist

This file is for the repository/source archive. It is not copied into the Thunderstore package.

## Before packaging

- [ ] `src/Plugin.cs` version matches `manifest.json`
- [ ] `CHANGELOG.md` has an entry for the release
- [ ] `README.md` matches the current feature set
- [ ] `icon.png` is 256x256
- [ ] GitHub `website_url` points to the real public repository
- [ ] Release build succeeds without errors
- [ ] No game assemblies or development files are included in the package

## Clean-profile test

- [ ] Create a fresh Thunderstore/r2modman Valheim profile
- [ ] Import `dist/Inject0rHUD-0.5.0.zip` as a local mod
- [ ] Launch Valheim
- [ ] F8 show/hide works
- [ ] F10 edit mode and settings work
- [ ] English is the default language on a fresh config
- [ ] RU / KK / ZH-CN can be switched live
- [ ] Positive effect timers count active duration, not boss-power cooldown
- [ ] Durability icons and values work
- [ ] FPS/Ping widgets work
- [ ] Profile create/save/import/export works
- [ ] Bush and crop hover timers work
- [ ] Beehive information works
- [ ] Fermenter information works
- [ ] Smelter/production information works
- [ ] Joining a normal vanilla dedicated server works without a server-side install

## Thunderstore page

- [ ] Correct team selected
- [ ] Package name: `Inject0rHUD`
- [ ] Version: `0.5.0`
- [ ] Dependency: `denikson-BepInExPack_Valheim-5.4.2350`
- [ ] Add relevant Valheim categories
- [ ] Upload a few real in-game screenshots if the community page supports them
- [ ] Re-read the current Thunderstore rules/categories immediately before publishing

## After publishing

- [ ] Install the public Thunderstore version into a fresh profile
- [ ] Verify the dependency is pulled automatically
- [ ] Verify the README renders correctly
- [ ] Verify the website/repository link works
