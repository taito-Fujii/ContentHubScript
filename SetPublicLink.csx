#load "./references/Action.csx"
/**------------ Include above to support intellisense on Content Hub types in editor ----------------**/
// Script Start
using System.Linq;
using System.Net;
using System.Net.Http;
using Stylelabs.M.Sdk.Models.Notifications;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Base.Querying.Filters;
using Stylelabs.M.Sdk.Contracts.Notifications;
using System.Globalization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;

MClient.Logger.Info("Public link rendition code started!");
var assetId = Context.TargetId.Value;
string result = await CreateRendition("preview", assetId);
MClient.Logger.Info("get from async:" + result);
// Logic appの処理短縮のため専用のエンティティを登録



// public linkの作成をする
async Task<string> CreateRendition(string rendition, long assetId)
{
    var publicLink = await MClient.EntityFactory.CreateAsync("M.PublicLink");
    if(publicLink.CanDoLazyLoading())
    {
        await publicLink.LoadMembersAsync(new PropertyLoadOption("Resource"), new RelationLoadOption("AssetToPublicLink"));
    }
    publicLink.SetPropertyValue("Resource",rendition);
    var relation = publicLink.GetRelation<IChildToManyParentsRelation>("AssetToPublicLink");
    if(relation == null)
    {
        MClient.Logger.Error("Unable to create public link: no AssetToPublicLink relation found.");
    }
    relation.Parents.Add(assetId);
    MClient.Logger.Debug($"Saving entity");
    MClient.Logger.Info($"publicLink:relation is {relation}");
    var entityId = await MClient.Entities.SaveAsync(publicLink);

   var entity = await MClient.Entities.GetAsync(entityId);
   var relativeUrl = await entity.GetPropertyValueAsync<string>("RelativeUrl");
   var versionHash = await entity.GetPropertyValueAsync<string>("VersionHash");

   return $"public link is <yourhost>/{relativeUrl}?v={versionHash}";
}
