
# OsuModder

>[!WARNING]
> This is Early Access.
> If you encountered any problems or bugs, please open issue or pr <3

>[!CAUTION]
> **YOU WILL GET PERMANENTLY BANNED, IF YOU ARE IN ONLINE MODE!** 
> 
> This project is considered as **unofficial, community-made tool**. Developer (me) is not responsible for your ban, if you will get one. **YOU HAVE BEEN WARNED**
<div align="center">

![osuModderBanner](Images/banner.png)
Mod-Loader for osu!lazer

</div>

## Why? (Please read it)
Recently I was curious to modificate UI for osu! in order to get some additional functionality. However, Recompiling osu!'s entire library is not quite promising, because I wanna share my code to my friends easily as well. I love playing with games' assembly, so I've decided to make entire mod-loader for it. :P

This project is not considered for hacks or anything related to unfair gameplay. 

I mainly made it for myself, but considered to turn it into the portfolio.

*No offence to the Peppy <3*

## Is this legal?
Yes. It is considered legal ONLY IF YOU'RE **NOT** CONNECTING TO THE OFFICIAL SERVERS! Game itself is open-source and licensed under MIT license.

**Please, be sure you are in offline mode before using this mod-loader.**

## Usage
IMPORTANT: Before modding, you must log out from osu account without remembering password option.

- Linux
	- you have to open `run.sh` and change `APPIMAGE_PATH=` to the osu's AppImage full path. You also can drop `osu.AppImage` into the directory where the script is. All patches will be happened in diffenet binary, so you can easily change between modded and original one (**consider your online access before changing!**)

- Windows
	- simply running script should be it. However, windows uses one binary for modded and original. If you wanna recover your osu and connect to the servers, check if you have `osu.Game.dll.ExtraBackup`. If you have it, delete `osu.Game.dll` and replace it with `osu.Game.dll.ExtraBackup` in order to be sure that you are running original/unpatched dll.

For modding, you putting `.dll` file (which is mod) into the `mods/` folder.
>[!NOTE]
>You must have `OsuModder.API.dll` in the mod folder, if mod requires it. It should be included with mod. Also, Code will generate backup of osu.Game.dll, if something will break.

Code will generate `mods/` folder in:

 - Linux:
   	 -  Appimage (Recommended): 
   	`~/.local/share/osu/mods`
   	  - Flatpak:
*No support yet*
 - Windows:
`C:\Users\{username}\AppData\Local\osulazer\mods\`

## Example
*This example is temporal! enriched one will be pushed very soon*

Please check [osu!Framework Documentation](https://github.com/ppy/osu-framework/wiki/Setting-up-your-first-project) in order to get started.

Anyway, let me show you an example:

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
So, that code affects ResultsScreen which adds button with function and removes everything like scores and stuff.
the `Action = () => ` from code simply means that if button will be clicked, it will execute that scope (for this example we used applause sound).

This is just very simple example of osu!Framework, which can be compiled and used as a mod for osu!
