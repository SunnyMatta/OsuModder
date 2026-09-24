
>[!WARNING]
> This is Early Access.
> If you encountered any problems or bugs, please open issue or pr <3

>[!CAUTION]
> **YOU WILL GET PERMANENTLY BANNED, IF YOU WILL USE IT IN ONLINE MODE!** 
> 
> This project is considered as **unofficial, community-made tool**. Developer (me) is not responsible for banishment or any liabilities. **YOU HAVE BEEN WARNED**
<div align="center">

![osuModderBanner](Images/banner.png)
Mod-Loader for osu!lazer

</div>

[![.NET](https://github.com/SunnyMatta/OsuModder/actions/workflows/dotnet.yml/badge.svg)](https://github.com/SunnyMatta/OsuModder/actions/workflows/dotnet.yml)

## Why? (Please read it)
Recently I was curious to create Mod-loader for no reason, and decided to make it for already open-source game called osu!lazer :P. This project is considered for personal education, any claims about how useless this project is will be ignored (because i know how useless it is for ordinary gamers, but not for programmers who want to learn stuff. you know? :3)

This project is **not** intended for any unfair gameplay modifications. 

I mainly made it for myself, yet considered to turn it into the portfolio.

*No offense to the Game Devs <3*

## Usage
>[!IMPORTANT]
> Third-party modification may contain malicious code. Do it on your own risk

- Linux
	- you have to open `run.sh` and change `APPIMAGE_PATH=` to the osu's AppImage full path. Dropping `osu.AppImage` into the script directory also valid. All patches will be happened in a different binary folder, hence your original `.AppImage` will be secured (**Consider your online access before any changes!**)

- Windows (**RISK INVOLVED**)
	- simply running script should be it. However, windows uses one binary for modded and original. In order to recover your osu without doubts, check if you have `osu.Game.dll.ExtraBackup` (code generates it after first execution). If file exists, replace `osu.Game.dll` with `osu.Game.dll.ExtraBackup` to be sure that you are running original/unpatched binaries.

For modding, you putting `.dll` file (which is mod) into the `mods/` folder.

>[!NOTE]
>Some modifications require `OsuModder.API.dll` in the mod folder. It should be included with mod.

Code will generate `mods/` folder in:

 - Linux:
   	 -  Appimage (Recommended): 
   	`~/.local/share/osu/mods`
   	  - Flatpak:
*Not supported yet*
 - Windows:
`C:\Users\{username}\AppData\Local\osulazer\mods\`

## Getting started

Please check [osu!Framework Documentation](https://github.com/ppy/osu-framework/wiki/Setting-up-your-first-project) in order to get started.

	using osu.Framework.Audio;
	using osu.Framework.Graphics;
	using osu.Framework.Graphics.Containers;
	using osu.Game.Audio;
	using osu.Game.Graphics.UserInterface;
	using osu.Game.Scoring;
	using osu.Game.Screens.Ranking;
	using osu.Game.Skinning;
	
	namespace osuMod
	{
		public partial class EpicCustomResultsScreen: ResultsScreen
		{
			public EpicCustomResultsScreen(ScoreInfo score): base(score)
			{
			}
			protected override void LoadComplete()
			{
				base.LoadComplete();
				
				// clears any internals from ResultScreen.
				ClearInternal();
				
				// initializing list of strings.
				var applauseSamples = new List<string>();
				
				// just a volume for applause
				const double applause_volume = 0.8f;
				
				// adding applause-s (version of applause which playing when you hitting s/ss score) sound to the list
				applauseSamples.Add(@"Results/applause-s");
				
				// initializing audio component
				var rankApplauseSound = new PoolableSkinnableSample(
				    new SampleInfo(applauseSamples.ToArray())
				);
				
				// AddInternal(); used for adding components to the scene you modding
				AddInternal(rankApplauseSound);
				AddInternal(new Container
				            {
					AutoSizeAxes = Axes.Both,
					Children = new Drawable[]
						   {
					new CustomOsuButton
					{
					Position = new osuTK.Vector2(10,10),
					Width = 200,
					Text = "meow",
					Action = () =>
						{
						rankApplauseSound.VolumeTo(applause_volume);
						rankApplauseSound.Play();
						}
				        }
				}
				});
			}
			private  partial  class  CustomOsuButton : OsuButton
			{
			}
		}
	}

This example affects ResultsScreen, which adds button with function and removes everything like scores and stuff.
the `Action = () => ` part from code simply means that if button will be clicked, it will execute that scope (for this example we used applause sound).

This is just very simple example of osu!Framework, which can be compiled and used as a binary for osuModder

## Legality
1. Trademark & Affiliation

    - OsuModder is an independent, community-developed open-source project. It is not affiliated with, endorsed by, or sponsored by Dean Herbert (peppy), ppy Pty Ltd, or the official osu! development team. All trademarks, registered trademarks, and game assets belong to their respective owners.

2. Terms of Service & Online Access

   - Client Integrity: Modifying game binaries or injecting foreign .dll files violates the osu! Terms of Service regarding client integrity when interacting with official infrastructure.

   - Offline Scope: This software is designed exclusively for offline UI experimentation, local feature testing, and educational research. Connecting to official osu! servers using a modified binaries will result an automated or manual permanent account ban.

3. Intellectual Property & Fair Use
   - No Asset Redistribution: OsuModder operates entirely as a dynamic patcher/loader and does not package, host, or redistribute copyrighted game assets, audio samples, or compiled game binaries belonging to ppy Pty Ltd.
   - Fair Use: Assembly hooking is performed locally on the user's machine for research, interoperability, and experimentation.
  
4. Contact & Copyright Notice
   - If you are a copyright holder or a representative of ppy Pty Ltd and have concerns regarding any aspect of this software, please open an issue or reach out directly to the maintainer for prompt resolution.
