using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace nivax.Data
{
    /// <summary>
    /// Base class for <see cref="SampleDataItem"/> and <see cref="SampleDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class SampleDataCommon : nivax.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public SampleDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(SampleDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem : SampleDataCommon
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, SampleDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private SampleDataGroup _group;
        public SampleDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup : SampleDataCommon
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex,Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<SampleDataItem> _items = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<SampleDataItem> _topItem = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> TopItems
        {
            get {return this._topItem; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// SampleDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _allGroups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<SampleDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");
            
            return _sampleDataSource.AllGroups;
        }

        public static SampleDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static SampleDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public SampleDataSource()
        {
            String ITEM_CONTENT = String.Format("Item Content: {0}",
                        "nivax");

            var group1 = new SampleDataGroup("Group-1",
                    "Nutrition",
                    "",
                    "Assets/10.png",
                    "");
            group1.Items.Add(new SampleDataItem("Group-1-Item-1",
                    "Introduction",
                    "",
                    "Assets/11.png",
                    "Nutrition (also called nourishment or aliment) is the provision, to cells and organisms, of the materials necessary (in the form of food) to support life.",
                    "Nutrition (also called nourishment or aliment) is the provision, to cells and organisms, of the materials necessary (in the form of food) to support life. Many common health problems can be prevented or alleviated with a healthy diet. The diet of an organism is what it eats, which is largely determined by the perceived palatability of foods. Dietitians are health professionals who specialize in human nutrition, meal planning, economics, and preparation. They are trained to provide safe, evidence-based dietary advice and management to individuals (in health and disease), as well as to institutions. Clinical nutritionists are health professionals who focus more specifically on the role of nutrition in chronic disease, including possible prevention or remediation by addressing nutritional deficiencies before resorting to drugs.",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-2",
                    "Overview",
                    "",
                    "Assets/12.png",
                    "Nutritional science investigates the metabolic and physiological responses of the body to diet.",
                    "Nutritional science investigates the metabolic and physiological responses of the body to diet. With advances in the fields of molecular biology, biochemistry, nutritional immunology, molecular medicine and genetics, the study of nutrition is increasingly concerned with metabolism and metabolic pathways: the sequences of biochemical steps through which substances in living things change from one form to another. Carnivore and herbivore diets are contrasting, with basic nitrogen and carbon proportions being at varying levels in particular foods. Carnivores consume more nitrogen than carbon[citation needed] while herbivores consume less nitrogen than carbon, when an equal quantity[which?] is measured.",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-3",
                    "Nutrients",
                    "",
                    "Assets/13.png",
                    "There are six major classes of nutrients: carbohydrates, fats, minerals, protein, vitamins, and water.",
                    "These nutrient classes can be categorized as either macronutrients (needed in relatively large amounts) or micronutrients (needed in smaller quantities). The macronutrients include carbohydrates (including fiber), fats, protein, and water. The micronutrients are minerals and vitamins.\n\nThe macronutrients (excluding fiber and water) provide structural material (amino acids from which proteins are built, and lipids from which cell membranes and some signaling molecules are built) and energy. Some of the structural material can be used to generate energy internally, and in either case it is measured in Joules or kilocalories (often called Calories and written with a capital C to distinguish them from little 'c' calories). Carbohydrates and proteins provide 17 kJ approximately (4 kcal) of energy per gram, while fats provide 37 kJ (9 kcal) per gram.,[17] though the net energy from either depends on such factors as absorption and digestive effort, which vary substantially from instance to instance. Vitamins, minerals, fiber, and water do not provide energy, but are required for other reasons. A third class of dietary material, fiber (i.e., non-digestible material such as cellulose), is also required,[citation needed] for both mechanical and biochemical reasons, although the exact reasons remain unclear.",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-4",
                    "Carbohydrates",
                    "",
                    "Assets/14.png",
                    "Carbohydrates may be classified as monosaccharides, disaccharides, or polysaccharides depending on the number of monomer (sugar) units they contain.",
                    "Carbohydrates may be classified as monosaccharides, disaccharides, or polysaccharides depending on the number of monomer (sugar) units they contain. They constitute a large part of foods such as rice, noodles, bread, and other grain-based products. Monosaccharides, disaccharides, and polysaccharides contain one, two, and three or more sugar units, respectively. Polysaccharides are often referred to as complex carbohydrates because they are typically long, multiple branched chains of sugar units.\n\nTraditionally, simple carbohydrates were believed to be absorbed quickly, and therefore to raise blood-glucose levels more rapidly than complex carbohydrates. This, however, is not accurate. Some simple carbohydrates (e.g. fructose) follow different metabolic pathways (e.g. fructolysis) which result in only a partial catabolism to glucose, while many complex carbohydrates may be digested at essentially the same rate as simple carbohydrates. Glucose stimulates the production of insulin through food entering the bloodstream, which is grasped by the beta cells in the pancreas.",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-5",
                    "Fiber",
                    "",
                    "Assets/15.png",
                    "Dietary fiber is a carbohydrate (or a polysaccharide) that is incompletely absorbed in humans and in some animals. Like all carbohydrates, when it is metabolized it can produce four Calories (kilocalories) of energy per gram.",
                    "Dietary fiber is a carbohydrate (or a polysaccharide) that is incompletely absorbed in humans and in some animals. Like all carbohydrates, when it is metabolized it can produce four Calories (kilocalories) of energy per gram. However, in most circumstances it accounts for less than that because of its limited absorption and digestibility. Dietary fiber consists mainly of cellulose, a large carbohydrate polymer that is indigestible because humans do not have the required enzymes to disassemble it. There are two subcategories: soluble and insoluble fiber. Whole grains, fruits (especially plums, prunes, and figs), and vegetables are good sources of dietary fiber. There are many health benefits of a high-fiber diet. Dietary fiber helps reduce the chance of gastrointestinal problems such as constipation and diarrhea by increasing the weight and size of stool and softening it. Insoluble fiber, found in whole wheat flour, nuts and vegetables, especially stimulates peristalsis – the rhythmic muscular contractions of the intestines which move digesta along the digestive tract. Soluble fiber, found in oats, peas, beans, and many fruits, dissolves in water in the intestinal tract to produce a gel which slows the movement of food through the intestines. This may help lower blood glucose levels because it can slow the absorption of sugar. Additionally, fiber, perhaps especially that from whole grains, is thought to possibly help lessen insulin spikes, and therefore reduce the risk of type 2 diabetes. The link between increased fiber consumption and a decreased risk of colorectal cancer is still uncertain.",
                    group1));
            this.AllGroups.Add(group1);

            var group2 = new SampleDataGroup("Group-2",
                    "Guidelines",
                    "",
                    "Assets/20.png",
                    "");
            group2.Items.Add(new SampleDataItem("Group-2-Item-1",
                    "Advice and guidance",
                    "",
                    "Assets/21.png",
                    "In the US, dietitians are registered (RD) or licensed (LD) with the Commission for Dietetic Registration and the American Dietetic Association, and are only able to use the title dietitian, as described by the business and professions codes of each respective state.",
                    "In the US, dietitians are registered (RD) or licensed (LD) with the Commission for Dietetic Registration and the American Dietetic Association, and are only able to use the title dietitian, as described by the business and professions codes of each respective state, when they have met specific educational and experiential prerequisites and passed a national registration or licensure examination, respectively. In California, registered dietitians must abide. Anyone may call themselves a nutritionist, including unqualified dietitians, as this term is unregulated. Some states, such as the State of Florida, have begun to include the title nutritionist in state licensure requirements. Most governments provide guidance on nutrition, and some also impose mandatory disclosure/labeling requirements for processed food manufacturers and restaurants to assist consumers in complying with such guidance.",
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-2",
                    "Teaching",
                    "",
                    "Assets/22.png",
                    "Nutrition is taught in schools in many countries. In England and Wales the Personal and Social Education and Food Technology curricula include nutrition, stressing the importance of a balanced diet and teaching how to read nutrition labels on packaging.",
                    "Nutrition is taught in schools in many countries. In England and Wales the Personal and Social Education and Food Technology curricula include nutrition, stressing the importance of a balanced diet and teaching how to read nutrition labels on packaging. In many schools a Nutrition class will fall within the Family and Consumer Science or Health departments. In some American schools, students are required to take a certain number of FCS or Health related classes. Nutrition is offered at many schools, and if it is not a class of its own, nutrition is included in other FCS or Health classes such as: Life Skills, Independent Living, Single Survival, Freshmen Connection, Health etc. In many Nutrition classes, students learn about the food groups, the food pyramid, Daily Recommended Allowances, calories, vitamins, minerals, malnutrition, physical activity, healthful food choices and how to live a healthy life.",
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-3",
                    "Healthy diet",
                    "",
                    "Assets/23.png",
                    "A healthy diet is one that helps maintain or improve general health. It is thought to be important for lowering health risks, such as obesity, heart disease, diabetes, hypertension and cancer.",
                    "A healthy diet is one that helps maintain or improve general health. It is thought to be important for lowering health risks, such as obesity, heart disease, diabetes, hypertension and cancer. A healthy diet involves consuming primarily fruits, vegetables, and whole grains to satisfy caloric requirements, provide the body with essential nutrients, phytochemicals, and fibre, and provide adequate water intake. A healthy diet supports energy needs and provides for human nutrition without exposure to toxicity or excessive weight gain from consuming excessive amounts.",
                    group2));
            this.AllGroups.Add(group2);

            var group3 = new SampleDataGroup("Group-3",
                    "Malnutrition",
                    "",
                    "Assets/30.png",
                    "");
            group3.Items.Add(new SampleDataItem("Group-3-Item-1",
                    "Perfect Way",
                    "",
                    "Assets/31.png",
                    "Malnutrition is the condition that results from taking an unbalanced diet in which certain nutrients are lacking, in excess (too high an intake), or in the wrong proportions.",
                    "Malnutrition is the condition that results from taking an unbalanced diet in which certain nutrients are lacking, in excess (too high an intake), or in the wrong proportions. A number of different nutrition disorders may arise, depending on which nutrients are under or overabundant in the diet. In most of the world, malnutrition is present in the form of undernutrition, which is caused by a diet lacking adequate calories and protein. While malnutrition is more common in developing countries, it is also present in industrialized countries. In wealthier nations it is more likely to be caused by unhealthy diets with excess energy, fats, and refined carbohydrates. A growing trend of obesity is now a major public health concern in lower socio-economic levels and in developing countries as well.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-2",
                    "Mortality",
                    "",
                    "Assets/32.png",
                    "According to Jean Ziegler(the United Nations Special Rapporteur on the Right to Food for 2000 to March 2008), mortality due to malnutrition accounted for 58 percent of the total mortality in 2006",
                    "According to Jean Ziegler(the United Nations Special Rapporteur on the Right to Food for 2000 to March 2008), mortality due to malnutrition accounted for 58 percent of the total mortality in 2006. In the world, approximately 62 million people, all causes of death combined, die each year. One in twelve people worldwide is malnourished and according to the Save the Children 2012 report, one in four of the world’s children are chronically malnourished. In 2006, more than 36 million died of hunger or diseases due to deficiencies in micronutrients.\n\nAccording to the World Health Organization, malnutrition is by far the biggest contributor to child mortality, present in half of all cases. Six million children die of hunger every year. Underweight births and intrauterine growth restrictions cause 2.2 million child deaths a year. Poor or non-existent breastfeeding causes another 1.4 million. Other deficiencies, such as lack of vitamin A or zinc, for example, account for 1 million. Malnutrition in the first two years is irreversible. Malnourished children grow up with worse health and lower educational achievements. Their own children also tend to be smaller. Malnutrition was previously seen as something that exacerbates the problems of diseases as measles, pneumonia and diarrhea. But malnutrition actually causes diseases as well, and can be fatal in its own right.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-3",
                    "Psychological",
                    "",
                    "Assets/33.png",
                    "Malnutrition in the form of iodine deficiency is the most common preventable cause of mental impairment worldwide.",
                    "Malnutrition in the form of iodine deficiency is the most common preventable cause of mental impairment worldwide. Even moderate iodine deficiency, especially in pregnant women and infants, lowers intelligence by 10 to 15 I.Q. points, shaving incalculable potential off a nation’s development.[40] The most visible and severe effects — disabling goiters, cretinism and dwarfism — affect a tiny minority, usually in mountain villages. But 16 percent of the world’s people have at least mild goiter, a swollen thyroid gland in the neck.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-4",
                    "Impact on learning",
                    "",
                    "Assets/34.png",
                    "Research indicates that improving the awareness of nutritious meal choices and establishing long-term habits of healthy eating has a positive effect on a cognitive and spatial memory capacity, potentially increasing a student's potential to process and retain academic information.",
                    "Research indicates that improving the awareness of nutritious meal choices and establishing long-term habits of healthy eating has a positive effect on a cognitive and spatial memory capacity, potentially increasing a student's potential to process and retain academic information.\n\nSome organizations have begun working with teachers, policymakers, and managed food service contractors to mandate improved nutritional content and increased nutritional resources in school cafeterias from primary to university level institutions. Health and nutrition have been proven to have close links with overall educational success. Better nourished children often perform significantly better in school, partly because they enter school earlier but mostly because of greater learning productivity per year of schooling. There is limited research available that directly links a student's Grade Point Average (G.P.A.) to their overall nutritional health. Additional substantive data are needed to prove that overall intellectual health is closely linked to a person's diet, rather than just another correlation fallacy.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-5",
                    "Metabolic syndrome",
                    "",
                    "Assets/35.png",
                    "Several lines of evidence indicate lifestyle-induced hyperinsulinemia and reduced insulin function (i.e. insulin resistance) as a decisive factor in many disease states.",
                    "Several lines of evidence indicate lifestyle-induced hyperinsulinemia and reduced insulin function (i.e. insulin resistance) as a decisive factor in many disease states. For example, hyperinsulinemia and insulin resistance are strongly linked to chronic inflammation, which in turn is associated with a variety of adverse developments such as arterial microinjuries and clot formation (i.e. heart disease) as well as exaggerated cell division (i.e. cancer). Hyperinsulinemia and insulin resistance (the so-called metabolic syndrome) are characterized by a combination of abdominal obesity, elevated blood sugar, elevated blood pressure, elevated blood triglycerides, and reduced HDL cholesterol. The negative impact of hyperinsulinemia on prostaglandin PGE1/PGE2 balance may be significant.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-6",
                    "Causes",
                    "",
                    "Assets/36.png",
                    "Major causes of malnutrition include poverty and food prices, dietary practices and agricultural productivity, with many individual cases being a mixture of several factors.",
                    "Major causes of malnutrition include poverty and food prices, dietary practices and agricultural productivity, with many individual cases being a mixture of several factors. Clinical malnutrition, such as in cachexia, is a major burden also in developed countries. Various scales of analysis also have to be considered in order to determine the sociopolitical causes of malnutrition. For example, the population of a community may be at risk if the area lacks health-related services, but on a smaller scale certain households or individuals may be at even higher risk due to differences in income levels, access to land, or levels of education.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-7",
                    "Poverty and food prices",
                    "",
                    "Assets/37.png",
                    "In Bangladesh, poor socioeconomic position was associated with chronic malnutrition since it inhibits purchase of nutritious foods such as milk, meat, poultry, and fruits.",
                    "In Bangladesh, poor socioeconomic position was associated with chronic malnutrition since it inhibits purchase of nutritious foods such as milk, meat, poultry, and fruits. As much as food shortages may be a contributing factor to malnutrition in countries with lack of technology, the FAO (Food and Agriculture Organization) has estimated that eighty percent of malnourished children living in the developing world live in countries that produce food surpluses. The economist Amartya Sen observed that, in recent decades, famine has always a problem of food distribution and/or poverty, as there has been sufficient food to feed the whole population of the world. He states that malnutrition and famine were more related to problems of food distribution and purchasing power.",
                    group3));
            this.AllGroups.Add(group3);
        }
    }
}
