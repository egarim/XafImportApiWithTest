using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Xpo;
using DevExpress.ExpressApp;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Spreadsheet;
using DevExpress.XtraPrinting.Native;
using System.IO;
using DevExpress.XtraSpreadsheet.Import.Xls;


namespace XafImportApiWithTest.Module.Import
{
    public enum PropertyKind
    {
        Primitive, Reference, Enum,

    }
    public class PropertyInfo
    {
        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
        public PropertyKind PropertyKind { get; set; }
        public string ReferencePropertyLookup { get; set; }
        public bool IsBusinessKey { get; set; }
  
    }
    public class RowDef
    {
        public string ObjectType { get; set; }
        public Dictionary<int, PropertyInfo> Properties { get; set; } = new Dictionary<int, PropertyInfo>();
        public List<List<object>> Rows { get; set; } = new List<List<object>>();
    }

    public class ExceptionData
    {
        public int RowNumber { get; set; }
        public Exception Exception { get; set; }
    }
    public class ImportResult
    {
        public TimeSpan TotalImportTime { get; set; }
        public int RowsInserted { get; set; }
        public int RowsUpdated { get; set; }
        List<ExceptionData> ExceptionData { get; set; } = new List<ExceptionData>();
        List<ExceptionData> ValidationErrors { get; set; } = new List<ExceptionData>();
        public ImportResult()
        {

        }
    }
    public enum ImportMode
    {
        InsertUpdate,
        Insert,
        Update
    }

    public class ImportService
    {
        List<XPCollection> objectsToUpdate = null;
    
        public ImportResult Import(IObjectSpace objectSpace, RowDef rowDef, ImportMode importMode)
        {
            int CurrentRowNumber = 0;
            int CurrentColumnNumber = 0;
            object CurrentValue=null;
            ImportResult importResult = new ImportResult();
            try
            {
                DateTime startTime = DateTime.Now;
                int ImportedObjects = 0;
               

                BinaryOperator CurrentOperator = null;

                List<KeyValuePair<int, PropertyInfo>> RefProperties = rowDef.Properties.Where(p => p.Value.PropertyKind == PropertyKind.Reference).ToList();
                var XpOs = objectSpace as XPObjectSpace;
                ITypesInfo TypesInfo = XpOs.TypesInfo;
                var PageSize = 1000;
                var Pages = GetPageSize(rowDef, PageSize);

                bool NoBusinessKey = true;
                Dictionary<PropertyInfo, List<XPCollection>> Collections = new Dictionary<PropertyInfo, List<XPCollection>>();
                List<XPCollection> AllCollections = new List<XPCollection>();

                KeyValuePair<int, PropertyInfo> Key = rowDef.Properties.FirstOrDefault(p => p.Value.IsBusinessKey);
                KeyValuePair<int, PropertyInfo> KeyProperty = new KeyValuePair<int, PropertyInfo>();
                if (rowDef.Properties.Any(p => p.Value.IsBusinessKey))
                {

                    NoBusinessKey = false;
                    KeyProperty = new KeyValuePair<int, PropertyInfo>(Key.Key, new PropertyInfo() { PropertyName = Key.Value.PropertyName, PropertyType = rowDef.ObjectType, PropertyKind = PropertyKind.Reference, ReferencePropertyLookup = Key.Value.PropertyName });
                    objectsToUpdate = BuildCollection(KeyProperty, rowDef, XpOs.Session, TypesInfo, PageSize, Pages);
                }


                foreach (KeyValuePair<int, PropertyInfo> RefProp in RefProperties)
                {
                    List<XPCollection> value = BuildCollection(RefProp, rowDef, XpOs.Session, TypesInfo, PageSize, Pages);
                    foreach (XPCollection xPCollection in value)
                    {
                        if (xPCollection == null)
                            continue;

                        AllCollections.Add(xPCollection);
                    }
                    Collections.Add(RefProp.Value, value);

                }
                if (objectsToUpdate != null)
                {
                    AllCollections.AddRange(objectsToUpdate);
                }

                XpOs.Session.BulkLoad(AllCollections.ToArray());

                Dictionary<int, object> IndexObject = new Dictionary<int, object>();
                foreach (List<object> Row in rowDef.Rows)
                {
                    var KeyValue = Row[Key.Key];
                    var Instance = GetObjectInstance(objectSpace, rowDef.ObjectType, TypesInfo, importMode, KeyValue, KeyProperty.Value?.PropertyName) as XPCustomObject;

                    IndexObject.Add(CurrentRowNumber, Instance);
                    CurrentColumnNumber = 0;

                    for (int i = 0; i < Row.Count; i++)
                    {
                        CurrentColumnNumber = i;
                        CurrentValue = Row[i];
#if DEBUG

                        Debug.WriteLine($"Current Index:{i} Current Property{rowDef.Properties[i].PropertyName}");
#endif


                       
                        switch (rowDef.Properties[i].PropertyKind)
                        {
                            case PropertyKind.Primitive:
                                Instance.SetMemberValue(rowDef.Properties[i].PropertyName, CurrentValue);
                                break;
                            case PropertyKind.Reference:
                                CurrentOperator = new BinaryOperator(rowDef.Properties[i].ReferencePropertyLookup, CurrentValue);
                                PropertyInfo propertyInfo = rowDef.Properties[i];
                                List<XPCollection> xPCollections = Collections[propertyInfo];
                                CurrentValue = GetValueFromCollection(CurrentOperator, xPCollections);
                                Instance.SetMemberValue(rowDef.Properties[i].PropertyName, CurrentValue);
                                break;
                            case PropertyKind.Enum:
                                var EnumTypeInfo = TypesInfo.FindTypeInfo(rowDef.Properties[i].PropertyType);
                                Instance.SetMemberValue(rowDef.Properties[i].PropertyName, Enum.Parse(EnumTypeInfo.Type, CurrentValue.ToString()));
                                break;
                        }
                        //if (rowDef.Properties[i].PropertyKind == PropertyKind.Primitive)
                        //    Instance.SetMemberValue(rowDef.Properties[i].PropertyName, Row[i]);
                        //else
                        //{
                        //    CurrentOperator = new BinaryOperator(rowDef.Properties[i].ReferencePropertyLookup, Row[i]);
                        //    PropertyInfo propertyInfo = rowDef.Properties[i];

                        //    List<XPCollection> xPCollections = Collections[propertyInfo];
                        //    object CurrentValue = GetValueFromCollection(CurrentOperator, xPCollections);
                        //    Instance.SetMemberValue(rowDef.Properties[i].PropertyName, CurrentValue);

                        //}
                    }

                    CurrentRowNumber++;
                    ImportedObjects++;
                }

                var Result = Validator.RuleSet.ValidateAllTargets(objectSpace, objectSpace.ModifiedObjects, "Save");
                if (Result.ValidationOutcome == ValidationOutcome.Valid)
                {

                    objectSpace.CommitChanges();
                }
                else
                {
                    IEnumerable<RuleSetValidationResultItem> Errors = Result.Results.Where(r => r.ValidationOutcome == ValidationOutcome.Error);
                    var Mo = objectSpace.ModifiedObjects.Count;
                    foreach (object item in Errors.Select(e => e.Target))
                    {
                        var Invalid = IndexObject.FirstOrDefault(Io => Io.Value == item);
                    }
                    var Mo2 = objectSpace.ModifiedObjects.Count;
                    //objectSpace.CommitChanges();
                }

                DateTime EndTime = DateTime.Now;
                importResult.TotalImportTime = EndTime - startTime;
            }
            catch (Exception ex)
            {

                throw new Exception($"Error on row {CurrentRowNumber} and column {CurrentColumnNumber} with value {CurrentValue}", ex);
            }

            return importResult;

        }

        protected virtual object GetObjectInstance(IObjectSpace objectSpace, string ObjectType, ITypesInfo TypesInfo, ImportMode importMode, object keyValue, string keyPropertyName)
        {
            BinaryOperator CurrentOperator = null;

            if (keyPropertyName == null)
            {
                CurrentOperator = new BinaryOperator(keyPropertyName, keyValue);
            }



            if (importMode == ImportMode.Insert)
            {
                return objectSpace.CreateObject(TypesInfo.FindTypeInfo(ObjectType).Type);
            }
            if (importMode == ImportMode.InsertUpdate)
            {
                if (this.objectsToUpdate != null)
                {

                    var returnValue = GetValueFromCollection(CurrentOperator, this.objectsToUpdate);
                    if (returnValue != null)
                        return returnValue;
                    else
                    {
                        return objectSpace.CreateObject(TypesInfo.FindTypeInfo(ObjectType).Type);
                    }
                }
                else
                {
                    return objectSpace.CreateObject(TypesInfo.FindTypeInfo(ObjectType).Type);
                }

            }
            if (importMode == ImportMode.Update)
            {
                if (CurrentOperator == null)
                    return null;

                return GetValueFromCollection(CurrentOperator, this.objectsToUpdate);
            }
            return null;

        }

        private object GetValueFromCollection(BinaryOperator CurrentOperator, List<XPCollection> xPCollections)
        {

            foreach (XPCollection xPCollection in xPCollections)
            {
                xPCollection.Filter = CurrentOperator;
                //Debug.WriteLine(CurrentOperator.ToString());

                if (xPCollection.Count > 0)
                {
                    //Debug.WriteLine("Value:" + xPCollection[0]);
                    return xPCollection[0];

                }

            }
            return null;


        }

        List<XPCollection> BuildCollection(KeyValuePair<int, PropertyInfo> RefProperties, RowDef rowDef, Session session, ITypesInfo TypesInfo, int pageSize, int pages)
        {
            List<XPCollection> collections = new List<XPCollection>();
            var CriteriaOperators = BuildCriteriaPages(RefProperties, rowDef, pageSize, pages);
            if (CriteriaOperators == null)
                return null;

            foreach (var Criteria in CriteriaOperators)
            {


                Type type = TypesInfo.FindTypeInfo(RefProperties.Value.PropertyType).Type;

                //Type genericListType = typeof(XPCollection<>);
                //Type concreteListType = genericListType.MakeGenericType(type);
                //XPCollection instance = Activator.CreateInstance(concreteListType, session) as XPCollection<>;
                //instance.TopReturnedObjects = 0;
                //instance.Criteria = Criteria;


                Debug.WriteLine($"Collection type:{type.FullName} Criteria:{Criteria}");
                var Collection = new XPCollection(session, type);
                //Collection.TopReturnedObjects = 0;
                Collection.Criteria = Criteria;
                //return Collection;
                //var tp = TypesInfo.FindTypeInfo(RefProperties.Value.PropertyType).Type;
                //var CurrentCollection = new XPCollection(PersistentCriteriaEvaluationBehavior.BeforeTransaction, session, tp, Criteria);
                //var CurrentCollection = new XPCollection(session, tp,false);
                //CurrentCollection.Criteria = Criteria;
                collections.Add(Collection);
            }
            return collections;

        }
        CriteriaOperator BuildCriteria(KeyValuePair<int, PropertyInfo> RefProperties, RowDef rowDef, int pageSize, int pages)
        {
            List<CriteriaOperator> operators = BuildCriteriaPages(RefProperties, rowDef, pageSize, pages);
            return CriteriaOperator.Or(operators);

        }

        private List<CriteriaOperator> BuildCriteriaPages(KeyValuePair<int, PropertyInfo> RefProperties, RowDef rowDef, int pageSize, int pages)
        {
            List<CriteriaOperator> operators = new List<CriteriaOperator>();
            for (int i = 0; i < pages; i++)
            {
                List<object> values = GetValues(rowDef, i + 1, pageSize, RefProperties.Key);
                operators.Add(new InOperator(RefProperties.Value.ReferencePropertyLookup, values));
            }

            return operators;
        }

        int GetPageSize(RowDef rowDef, int pageSize)
        {
            return (int)Math.Ceiling((double)rowDef.Rows.Count() / pageSize);
        }
        List<Object> GetValues(RowDef rowDef, int page, int pageSize, int ColumIndex)
        {
            Debug.WriteLine($"Page: {page} ColumIndex: {ColumIndex}");
            return rowDef.Rows
                .Select(innerList => innerList[ColumIndex])
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
    }
    public class PropertyDetails
    {
        public Type OwnerType { get; set; }
        public Type PropertyType { get; set; }
        public string PropertyName { get; set; }
    }
    public class SpreadsheetService
    {
        public Workbook LoadWorkbookFromBytes(byte[] data, DocumentFormat documentFormat)
        {
            Workbook workbook = new Workbook();

            workbook.LoadDocument(new MemoryStream(data), documentFormat);
            return workbook;
        }
        public SpreadsheetService()
        {
            
        }

        public RowDef GetData(Worksheet worksheet, ITypesInfo typesInfo, Type spreadSheetType)
        {
            var Headers = GetHeaders(worksheet);
            RowDef rowDef = new RowDef();

            GetRowDefinitionProperties(spreadSheetType, Headers, rowDef);
            rowDef.Rows= GetValues(worksheet);

            return rowDef;
        }
        List<List<object>> GetValues(Worksheet worksheet)
        {
            // Get the used range of the worksheet.
            CellRange usedRange = worksheet.GetUsedRange();

            // Initialize a list to hold a list of objects for each row.
            List<List<object>> allRowsData = new List<List<object>>();

            // Iterate through all rows in the used range.
            for (int rowIndex = usedRange.TopRowIndex; rowIndex <= usedRange.BottomRowIndex; rowIndex++)
            {
                // Initialize a list to hold the data for the current row.
                List<object> rowData = new List<object>();

                // Iterate through all columns in the current row.
                for (int columnIndex = usedRange.LeftColumnIndex; columnIndex <= usedRange.RightColumnIndex; columnIndex++)
                {
                    // Access the cell in the current row and column.
                    Cell cell = worksheet.Cells[rowIndex, columnIndex];

                    object cellValue = null;
                    CellValue value = cell.Value;

                    switch (value.Type)
                    {
                        case CellValueType.Boolean:
                            cellValue = value.BooleanValue;
                            break;
                        case CellValueType.DateTime:
                            cellValue = value.DateTimeValue;
                            break;
                        case CellValueType.Numeric:
                            //cellValue = cell.DisplayText;  //TODO fix after the test,for the example we are going to use text representation
                            cellValue = value.NumericValue;
                            break;
                        case CellValueType.Error:
                            // Handle the error value appropriately.
                            cellValue = value.ErrorValue;
                            break;
                        case CellValueType.Text:
                        default:
                            // For text and any other types not explicitly handled above.
                            cellValue = value.IsEmpty ? null : value.TextValue;
                            break;
                    }
                    //Debug.WriteLine(cell.Value.GetType().FullName);
                    // Add the cell's value to the current row's data list.
                    rowData.Add(cellValue);
                }

                // Add the current row's data list to the list of all rows' data.
                allRowsData.Add(rowData);
            }
            allRowsData.Remove(allRowsData.First());
            return allRowsData;
        }
        private static void GetRowDefinitionProperties(Type spreadSheetType, List<string> Headers, RowDef rowDef)
        {
            Dictionary<string, List<PropertyDetails>> propertyDetails = GetPropertyDetails(spreadSheetType, Headers);

            //var typeInfo = typesInfo.FindTypeInfo(spreadSheetType);

            int Index = 0;
            foreach (KeyValuePair<string, List<PropertyDetails>> PropertyAndPaths in propertyDetails)
            {
                if (PropertyAndPaths.Value.Count() > 1)
                {
                    PropertyDetails LastSegment = PropertyAndPaths.Value.Last();
                    PropertyDetails RootProperty = PropertyAndPaths.Value[0];
                    PropertyInfo PropertyInfoInstance = new PropertyInfo() { PropertyName = RootProperty.PropertyName, PropertyType = RootProperty.PropertyType.FullName, ReferencePropertyLookup = LastSegment.PropertyName, PropertyKind = PropertyKind.Reference };
                    rowDef.Properties.Add(Index, PropertyInfoInstance);
                }
                else
                {
                    PropertyKind propertyKind= PropertyKind.Primitive;
                    if (PropertyAndPaths.Value.Last().PropertyType.IsEnum)
                        propertyKind= PropertyKind.Enum;

                    rowDef.Properties.Add(Index, new PropertyInfo() { PropertyName = PropertyAndPaths.Key, PropertyType = PropertyAndPaths.Value.Last().PropertyType.FullName, PropertyKind = propertyKind });
                }
                Index++;
            }
        }

        private static Dictionary<string, List<PropertyDetails>> GetPropertyDetails(Type spreadSheetType, List<string> Headers)
        {
            Dictionary<string, List<PropertyDetails>> propertyDetails = new Dictionary<string, List<PropertyDetails>>();
            foreach (var item in Headers)
            {
                propertyDetails.Add(item, GetPropertyDetails(item, spreadSheetType));
            }

            return propertyDetails;
        }
        public static class NullableChecker
        {
            public static bool IsNullable(System.Reflection.PropertyInfo propertyInfo)
            {
                // Check if the property is a Nullable value type.
                if (Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null)
                {
                    return true;
                }

                // Check for reference type or Nullable reference type (C# 8.0+ feature).
                // If the project does not use nullable reference types, this part can be omitted.
                if (!propertyInfo.PropertyType.IsValueType)
                {
                    // Check for nullable attribute.
                    var nullableAttribute = propertyInfo.CustomAttributes
                        .FirstOrDefault(attr => attr.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
                    if (nullableAttribute != null && nullableAttribute.ConstructorArguments.Count > 0)
                    {
                        var attributeArgument = (byte[])nullableAttribute.ConstructorArguments[0].Value;
                        if (attributeArgument.Length > 0 && attributeArgument[0] == 2)
                        {
                            return true; // Property is annotated as nullable reference type.
                        }
                    }
                    else
                    {
                        // If the nullable context is not enabled or the property is not annotated,
                        // reference types are considered nullable by default.
                        return true;
                    }
                }

                return false; // The property is non-nullable value type or non-nullable reference type.
            }
        }
        public static List<PropertyDetails> GetPropertyDetails(string propertyPath,Type currentType)
        {
            var detailsList = new List<PropertyDetails>();
            string[] properties = propertyPath.Split('.');
            

            foreach (string propertyName in properties)
            {
                System.Reflection.PropertyInfo propertyInfo = currentType.GetProperty(propertyName);
                bool isNullable = NullableChecker.IsNullable(propertyInfo);
                Type PropertyType = null; ;
                if (isNullable)
                {
                  
                    PropertyType = Nullable.GetUnderlyingType(propertyInfo.PropertyType);
                    if(PropertyType==null)
                    {
                        PropertyType = propertyInfo.PropertyType;
                    }
                }
                else
                {
                    PropertyType = propertyInfo.PropertyType;
                }
               
              


                if (propertyInfo == null)
                {
                    throw new Exception($"Property {propertyName} not found on {currentType.Name}.");
                }

                var details = new PropertyDetails
                {
                    OwnerType = currentType,
                    PropertyType = PropertyType,//propertyInfo.PropertyType,
                    PropertyName = propertyInfo.Name
                };
                detailsList.Add(details);

                currentType = propertyInfo.PropertyType;
            }

            return detailsList;
        }
        public List<string> GetHeaders(Worksheet worksheet)
        {

            List<string> Headers = new List<string>();
            // Get the used range of the worksheet.
            CellRange usedRange = worksheet.GetUsedRange();

            // Get the first row from the used range.
            Row firstRow = worksheet.Rows[usedRange.TopRowIndex];

            // Iterate through all the columns in the first row of the used range.
            for (int columnIndex = usedRange.LeftColumnIndex; columnIndex <= usedRange.RightColumnIndex; columnIndex++)
            {
                // Access the cell in the first row at the columnIndex.
                Cell cell = firstRow[columnIndex];

                // Read the cell value.
                Headers.Add(cell.DisplayText);
                // Do something with the cell value, such as printing it out.
                Debug.WriteLine($"Column {columnIndex + 1}: {cell.DisplayText}");
            }
            return Headers;





        }
    }

}
