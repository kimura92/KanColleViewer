using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.Model;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using Livet;
using Livet.EventListeners;
using Livet.Messaging.Windows;

namespace Grabacr07.KanColleViewer.ViewModels.Contents.Fleets
{
	/// <summary>
	/// 艦隊の再出撃に関する情報を提供します。
	/// </summary>
	public class ReSortieBarViewModel : ViewModel
	{
		private readonly FleetReSortie source;

		#region CanReSortie 変更通知プロパティ

		private bool _CanReSortie;

		public bool CanReSortie
		{
			get { return this._CanReSortie; }
			set
			{
				if (this._CanReSortie != value)
				{
					this._CanReSortie = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region Message 変更通知プロパティ

		private string _Message;

		public string Message
		{
			get { return this._Message; }
			set
			{
				if (this._Message != value)
				{
					this._Message = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region Remaining 変更通知プロパティ

		private string _Remaining;

		public string Remaining
		{
			get { return this._Remaining; }
			set
			{
				if (this._Remaining != value)
				{
					this._Remaining = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region IsNotifyReadied 変更通知プロパティ

		private bool _IsNotifyReadied;

		public bool IsNotifyReadied
		{
			get { return this._IsNotifyReadied; }
			set
			{
				if (this._IsNotifyReadied != value)
				{
					this._IsNotifyReadied = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		public ReSortieBarViewModel(FleetViewModel parent, FleetReSortie reSortie)
		{
			this.source = reSortie;
			this.CompositeDisposable.Add(new PropertyChangedEventListener(reSortie)
			{
				{ () => reSortie.Reason, (sender, args) => this.UpdateMessage() },
				{ () => reSortie.Remaining, (sender, args) => this.UpdateRemaining() },
			});

			this.UpdateMessage();
			this.UpdateRemaining();

			if (Toast.IsSupported)
			{
				reSortie.Readied += (sender, args) =>
				{
					if (this.IsNotifyReadied)
					{
						Toast.Show(
							"Morale Recovery",
							"Morale of ships in the " + parent.Name + "has fully recovered.",
							() => App.ViewModelRoot.Activate());
					}
				};
			}
			else
			{
				reSortie.Readied += (sender, args) =>
				{
					if (this.IsNotifyReadied)
					{
						NotifyIconWrapper.Show(
							"Morale Recovery",
							"Morale of ships in the " + parent.Name + "has fully recovered.");
					}
				};
			}
		}


		private void UpdateMessage()
		{
			if (this.source.CanReSortie)
			{
				this.Message = "Ready to sortie!";
				this.CanReSortie = true;
				return;
			}

			var list = new List<string>();

			if (this.source.Reason.HasFlag(CanReSortieReason.Wounded))
			{
				list.Add("severely damaged");
			}
			if (this.source.Reason.HasFlag(CanReSortieReason.LackForResources))
			{
				list.Add("inadequate supplies");
			}
			if (this.source.Reason.HasFlag(CanReSortieReason.BadCondition))
			{
				list.Add("low morale");
			}

			this.Message = string.Format("Active condition: {0}.", list.ToString(", "));
			this.CanReSortie = false;
		}

		private void UpdateRemaining()
		{
			this.Remaining = this.source.Remaining.HasValue ? "\nMorale will be fully restored in: " + this.source.Remaining.Value.ToString(@"mm\:ss") : "";
		}
	}
}
