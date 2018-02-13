using CoreSampleAnnotation.AnnotationPlane.ColumnSettings;
using CoreSampleAnnotation.AnnotationPlane.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane
{
    public static class PlaneHalpers
    {
        public static LayerClassVM ClassToClassVM(Class cl)
        {
            LayerClassVM result = new LayerClassVM(cl.ID);
            if (cl.Acronym != null)
                result.Acronym = cl.Acronym;
            if (cl.Color != null && cl.Color.HasValue)
                result.Color = cl.Color.Value;
            if (cl.Description != null)
                result.Description = cl.Description;
            if (cl.ShortName != null)
                result.ShortName = cl.ShortName;
            return result;

        }

        public static PlaneVM BuildPlane(LayersAnnotation annotation, Property[] template, ColumnSettingsVM columnDefinitions, Intervals.PhotoRegion[] photos)
        {
            PlaneVM vm = new PlaneVM();

            double upperDepth = annotation.LayerBoundaries[0];
            double lowerDepth = annotation.LayerBoundaries[annotation.LayerBoundaries.Length - 1];

            vm.ColScaleController.UpperDepth = upperDepth;
            vm.LayerSyncController.UpperDepth = upperDepth;
            vm.ColScaleController.LowerDepth = lowerDepth;
            vm.LayerSyncController.LowerDepth = lowerDepth;
            vm.LayerSyncController.SetColumnDepth(upperDepth,lowerDepth);

            Dictionary<string, LayeredColumnVM> propColumns = new Dictionary<string, LayeredColumnVM>();

            double colHeight = vm.LayerSyncController.DepthToWPF(lowerDepth) - vm.LayerSyncController.DepthToWPF(upperDepth);

            int layersCount = annotation.LayerBoundaries.Length - 1;

            foreach (var columnDefinition in columnDefinitions.OrderedColumnDefinitions)
            {
                if (columnDefinition is DepthColumnDefinitionVM)
                {
                    DepthAxisColumnVM colVM = new DepthAxisColumnVM("Шкала глубин");
                    colVM.ColumnHeight = colHeight;
                    vm.AnnoGridVM.Columns.Add(colVM);
                    vm.ColScaleController.AttachToColumn(new ColVMAdapter(colVM));                    
                }
                else if (columnDefinition is LayerLengthColumnDefinitionVM)
                {
                    LayerRealLengthColumnVM colVM = new LayerRealLengthColumnVM("Мощность эл-та циклита (м)");
                    colVM.ColumnHeight = colHeight;
                    for (int i = 0; i < layersCount; i++)
                    {
                        LengthLayerVM llVM = new LengthLayerVM();
                        llVM.Length = vm.LayerSyncController.LengthToWPF(annotation.LayerBoundaries[i + 1] - annotation.LayerBoundaries[i]);
                        colVM.Layers.Add(llVM);
                    }
                    vm.AnnoGridVM.Columns.Add(colVM);
                    vm.ColScaleController.AttachToColumn(new ColVMAdapter(colVM));
                    vm.LayerSyncController.RegisterLayer(new SyncronizerColumnAdapter(colVM));
                }
                else if (columnDefinition is PhotoColumnDefinitionVM)
                {
                    ImageColumnVM imColVM = new ImageColumnVM("Фото керна");
                    imColVM.ColumnHeight = colHeight;
                    imColVM.ImageRegions = photos;
                    vm.AnnoGridVM.Columns.Add(imColVM);
                    vm.ColScaleController.AttachToColumn(new ColVMAdapter(imColVM));

                }
                else if (columnDefinition is LayeredTextColumnDefinitionVM)
                {
                    LayeredTextColumnDefinitionVM colDef = (LayeredTextColumnDefinitionVM)columnDefinition;
                    if (colDef.SelectedCentreTextProp != null)
                    {
                        string propID = colDef.SelectedCentreTextProp.PropID;
                        
                        LayeredColumnVM propColumnVM; //the one which holds actual prop value, can be shared between multiple presentation columns
                        //first finding corresponding prop column
                        if (propColumns.ContainsKey(propID))
                        {
                            propColumnVM = propColumns[propID];
                        }
                        else { //or creating it, if it is not created yet                            
                            propColumnVM = new LayeredColumnVM(propID);
                            propColumnVM.ColumnHeight = colHeight;
                            propColumns.Add(propID, propColumnVM);

                            //preparing available classes                        
                            Property prop = template.Where(p => p.ID == propID).Single();
                            List<LayerClassVM> availableClasses = new List<LayerClassVM>();
                            foreach (Class cl in prop.Classes)
                            {
                                LayerClassVM lcVM = ClassToClassVM(cl);
                                availableClasses.Add(lcVM);
                            }

                            //setting selected class
                            for (int i = 0; i < layersCount; i++)
                            {
                                ClassificationLayerVM clVM = new ClassificationLayerVM();
                                clVM.Length = vm.LayerSyncController.LengthToWPF(annotation.LayerBoundaries[i + 1] - annotation.LayerBoundaries[i]);
                                clVM.PossibleClasses = new System.Collections.ObjectModel.ObservableCollection<LayerClassVM>(availableClasses);
                                var layerAnnotation = annotation.LayerAnnotation[i];

                                if (layerAnnotation.ContainsKey(prop.ID))
                                {
                                    string[] selected = layerAnnotation[prop.ID];
                                    if (selected.Length > 1)
                                        throw new NotSupportedException();
                                    string selected1 = selected[0];
                                    clVM.CurrentClass = availableClasses.Find(e => e.ID == selected1);
                                }

                                propColumnVM.Layers.Add(clVM);
                            }

                            vm.LayerSyncController.RegisterLayer(new SyncronizerColumnAdapter(propColumnVM));
                            vm.ColScaleController.AttachToColumn(new ColVMAdapter(propColumnVM));
                        }

                        //the prop column (shared between multiple presenations) is ready.

                        //Creating presentation column.

                        //here is the actual displayed text is set
                        Func<LayerClassVM, string> centreTextExtractor;
                        switch (colDef.SelectedCentreTextProp.Presentation)
                        {
                            case Presentation.Acronym: centreTextExtractor = vm1 => vm1.Acronym; break;
                            case Presentation.Description: centreTextExtractor = vm1 => vm1.Description; break;
                            case Presentation.ShortName: centreTextExtractor = vm1 => vm1.ShortName; break;
                            case Presentation.Colour: throw new InvalidOperationException();
                            default: throw new NotImplementedException();
                        }

                        LayeredPresentationColumnVM colVM = new LayeredPresentationColumnVM(
                            colDef.SelectedCentreTextProp.TexturalString,
                            propColumnVM,
                            lVM => new ClassificationLayerTextPresentingVM((ClassificationLayerVM)lVM) { TextExtractor = centreTextExtractor });

                        colVM.ColumnHeight = colHeight;

                        vm.AnnoGridVM.Columns.Add(colVM);                                
                    }                    
                }
                else throw new NotSupportedException("Незнакомое определение колонки");
            }
            return vm;
        }
    }
}
