﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Nodes.DBHelper;
using Nodes.Entities;
using Nodes.Utils;
using Nodes.UI;
using DevExpress.XtraEditors;
using Nodes.Shares;
using System.Data;
using Nodes.Entities.HttpEntity;
using Nodes.Entities.HttpEntity.BaseData;
using Nodes.Entities.HttpEntity.C02.BaseData;
using Newtonsoft.Json;

namespace Nodes.BaseData
{
    public partial class FrmLocationEditType_C : DevExpress.XtraEditors.XtraForm
    {
        private LocationEntity locationEntity = null;
        public event EventHandler DataSourceChanged = null;
        private bool isNew = true;
        private LocationDal locationDal = null;
        private ZoneDal zoneDal = null;
        //private ChannelDal channelDal = null;
        private DataTable dtChannel = new DataTable();
        public FrmLocationEditType_C()
        {
            InitializeComponent();
        }

        public FrmLocationEditType_C(LocationEntity locationEntity)
            : this()
        {
            this.locationEntity = locationEntity;
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            locationDal = new LocationDal();
            zoneDal = new ZoneDal();
            //channelDal = new ChannelDal();
            BindingCombox();
            BindChannel(locationEntity);
            if (locationEntity != null)
            {
                this.Text = "货位-修改";
                txtCode.Enabled = false;
                layoutControlItem3.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                ShowEditInfo(locationEntity);
                isNew = false;
            }
        }

        #region 自定义方法

        ///<summary>
        ///查询所有货区
        ///</summary>
        ///<returns></returns>
        public List<ZoneEntity> GetAllZone()
        {
            List<ZoneEntity> list = new List<ZoneEntity>();
            try
            {
                #region 请求数据
                System.Text.StringBuilder loStr = new System.Text.StringBuilder();
                //loStr.Append("vhNo=").Append(vehicleNO);
                string jsons = string.Empty;
                string jsonQuery = WebWork.SendRequest(jsons, WebWork.URL_GetAllZone);
                if (string.IsNullOrEmpty(jsonQuery))
                {
                    MsgBox.Warn(WebWork.RESULT_NULL);
                    //LogHelper.InfoLog(WebWork.RESULT_NULL);
                    return list;
                }
                #endregion

                #region 正常错误处理

                JsonGetAllZone bill = JsonConvert.DeserializeObject<JsonGetAllZone>(jsonQuery);
                if (bill == null)
                {
                    MsgBox.Warn(WebWork.JSON_DATA_NULL);
                    return list;
                }
                if (bill.flag != 0)
                {
                    MsgBox.Warn(bill.error);
                    return list;
                }
                #endregion

                #region 赋值数据
                foreach (JsonGetAllZoneResult jbr in bill.result)
                {
                    ZoneEntity asnEntity = new ZoneEntity();
                    asnEntity.IsActive = jbr.isActive;
                    asnEntity.TemperatureCode = jbr.tempCode;
                    asnEntity.TemperatureName = jbr.tempName;
                    asnEntity.WarehouseCode = jbr.whCode;
                    asnEntity.WarehouseName = jbr.whName;
                    asnEntity.ZoneCode = jbr.znCode;
                    asnEntity.ZoneName = jbr.znName;
                    asnEntity.ZoneTypeCode = jbr.ztCode;
                    asnEntity.ZoneTypeName = jbr.ztName;
                    list.Add(asnEntity);
                }
                return list;
                #endregion
            }
            catch (Exception ex)
            {
                MsgBox.Err(ex.Message);
            }
            return list;
        }

        private void BindingCombox()
        {
            listZones.Properties.DataSource = GetAllZone();
            //listUnitGroup.Properties.DataSource = new UnitGroupDal().GetAllActive();
        }

        private void ShowEditInfo(LocationEntity locEntity)
        {
            txtCode.Text = locEntity.LocationCode;
            txtName.Text = locEntity.LocationName;
            listZones.EditValue = locEntity.ZoneCode;
            txtPassage.Text = locEntity.PassageCode;
            txtFloor.Text = locEntity.FloorCode;
            txtShelf.Text = locEntity.ShelfCode;
            txtCellCode.Text = locEntity.CellCode;
            spinSortOrder.Value = locEntity.SortOrder;

            comboIsActive.Text = locEntity.IsActive;

            spinLowerSize.Value = locEntity.LowerSize;
            spinUpperSize.Value = locEntity.UpperSize;

            listUnitGroup.EditValue = locEntity.GrpCode;
            listUnits.EditValue = locEntity.UnitCode;
            lookUpChannel.Text = locEntity.Ch_Name;
        }

        private void Continue()
        {
            txtName.Text = "";
            spinLowerSize.Value = spinUpperSize.Value = 0;
            listUnits.EditValue = null;

            if (checkAutoIncrement.Checked)
            {
                txtCode.Text = AutoIncrement.NextCode(txtCode.Text.Trim());
                txtName.Focus();
            }
            else
            {
                txtCode.Text = "";
                txtCode.Focus();
            }
        }

        //货位名称可以为空，没什么用
        private bool IsFieldValueValid()
        {
            if (string.IsNullOrEmpty(txtCode.Text))
            {
                MsgBox.Warn("货位编码不能为空。");
                return false;
            }

            if (listZones.EditValue == null)
            {
                MsgBox.Warn("请选择所属货区。");
                return false;
            }

            if (spinUpperSize.Value < spinLowerSize.Value)
            {
                MsgBox.Warn("库容下限不能大于上限。");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 基础管理（货位信息-添加货位）
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="isNew"></param>
        /// <returns></returns>
        public bool SaveAddLocationInfoC02(LocationEntity entity, bool isNew)
        {
            try
            {
                #region 请求数据
                System.Text.StringBuilder loStr = new System.Text.StringBuilder();
                #region 0-10
                loStr.Append("lcCode=").Append(entity.LocationCode).Append("&");
                loStr.Append("lcName=").Append(entity.LocationName).Append("&");
                loStr.Append("znCode=").Append(entity.ZoneCode).Append("&");
                loStr.Append("passageCode=").Append(entity.PassageCode).Append("&");
                loStr.Append("floorCode=").Append(entity.FloorCode).Append("&");
                loStr.Append("shelfCode=").Append(entity.ShelfCode).Append("&");
                loStr.Append("cellCode=").Append(entity.CellCode).Append("&");
                loStr.Append("sortOrder=").Append(entity.SortOrder).Append("&");
                loStr.Append("whCode=").Append(entity.WarehouseCode).Append("&");
                loStr.Append("isActive=").Append(entity.IsActive).Append("&");
                #endregion

                #region 11-14
                loStr.Append("lowerSize=").Append(entity.LowerSize).Append("&");
                loStr.Append("upperSize=").Append(entity.UpperSize).Append("&");
                loStr.Append("ugCode=").Append(entity.GrpCode).Append("&");
                loStr.Append("chCode=").Append(entity.Ch_Code).Append("&");
                loStr.Append("chName=").Append(entity.Ch_Name).Append("&");
                loStr.Append("umCode=").Append(entity.UnitCode);
                #endregion
                string jsonQuery = WebWork.SendRequest(loStr.ToString(), WebWork.URL_SaveAddLocationInfoC02);
                if (string.IsNullOrEmpty(jsonQuery))
                {
                    MsgBox.Warn(WebWork.RESULT_NULL);
                    //LogHelper.InfoLog(WebWork.RESULT_NULL);
                    return false;
                }
                #endregion

                #region 正常错误处理

                Sucess bill = JsonConvert.DeserializeObject<Sucess>(jsonQuery);
                if (bill == null)
                {
                    MsgBox.Warn(WebWork.JSON_DATA_NULL);
                    return false;
                }
                if (bill.flag != 0)
                {
                    MsgBox.Warn(bill.error);
                    return false;
                }
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                MsgBox.Err(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// 基础管理（货位信息-编辑货位）
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="isNew"></param>
        /// <returns></returns>
        public bool UpdateLocationInfoC02(LocationEntity entity, bool isNew)
        {
            try
            {
                #region 请求数据
                System.Text.StringBuilder loStr = new System.Text.StringBuilder();
                #region 0-10
                loStr.Append("lcCode=").Append(entity.LocationCode).Append("&");
                loStr.Append("lcName=").Append(entity.LocationName).Append("&");
                loStr.Append("znCode=").Append(entity.ZoneCode).Append("&");
                loStr.Append("passageCode=").Append(entity.PassageCode).Append("&");
                loStr.Append("floorCode=").Append(entity.FloorCode).Append("&");
                loStr.Append("shelfCode=").Append(entity.ShelfCode).Append("&");
                loStr.Append("cellCode=").Append(entity.CellCode).Append("&");
                loStr.Append("sortOrder=").Append(entity.SortOrder).Append("&");
                loStr.Append("whCode=").Append(entity.WarehouseCode).Append("&");
                loStr.Append("isActive=").Append(entity.IsActive).Append("&");
                #endregion

                #region 11-14
                loStr.Append("lowerSize=").Append(entity.LowerSize).Append("&");
                loStr.Append("upperSize=").Append(entity.UpperSize).Append("&");
                loStr.Append("ugCode=").Append(entity.GrpCode).Append("&");
                loStr.Append("chCode=").Append(entity.Ch_Code).Append("&");
                loStr.Append("chName=").Append(entity.Ch_Name).Append("&");
                loStr.Append("umCode=").Append(entity.UnitCode);
                #endregion
                string jsonQuery = WebWork.SendRequest(loStr.ToString(), WebWork.URL_UpdateLocationInfoC02);
                if (string.IsNullOrEmpty(jsonQuery))
                {
                    MsgBox.Warn(WebWork.RESULT_NULL);
                    //LogHelper.InfoLog(WebWork.RESULT_NULL);
                    return false;
                }
                #endregion

                #region 正常错误处理

                Sucess bill = JsonConvert.DeserializeObject<Sucess>(jsonQuery);
                if (bill == null)
                {
                    MsgBox.Warn(WebWork.JSON_DATA_NULL);
                    return false;
                }
                if (bill.flag != 0)
                {
                    MsgBox.Warn(bill.error);
                    return false;
                }
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                MsgBox.Err(ex.Message);
            }

            return false;
        }

        private bool Save()
        {
            if (!IsFieldValueValid()) return false;
            bool success = false;
            try
            {
                LocationEntity editEntity = PrepareSave();
                //int ret = locationDal.Save(editEntity, isNew);
                //if (ret == -1)
                //    MsgBox.Warn("货位编号已存在，请改为其他的编号。");
                //else if (ret == -2)
                //    MsgBox.Warn("更新失败，该行已经被其他人删除。");
                //else if (ret == -3)
                //    MsgBox.Warn("货位有库存，不允许修改。");
                //else
                bool ret;
                if (isNew)
                    ret = SaveAddLocationInfoC02(editEntity, isNew);
                else
                    ret = UpdateLocationInfoC02(editEntity, isNew);

                if (ret)
                {
                    success = true;
                    if (DataSourceChanged != null)
                    {
                        DataSourceChanged(editEntity, null);
                    }
                }
            }
            catch (Exception ex)
            {
                MsgBox.Err(ex.Message);
            }
            return success;
        }
        #endregion

        /// <summary>
       /// 返回通道的编码、名称、备用编码、是否启用
       /// </summary>
       /// <returns></returns>
        public DataTable GetChannel()
        {
            #region
            DataTable tblDatas = new DataTable("Datas");
            tblDatas.Columns.Add("CH_CODE", Type.GetType("System.String"));
            tblDatas.Columns.Add("CH_NAME", Type.GetType("System.String"));
            tblDatas.Columns.Add("BAK_CH_NAME", Type.GetType("System.String"));
            tblDatas.Columns.Add("IS_ACTIVE", Type.GetType("System.String"));
            tblDatas.Columns.Add("BAK_CH_CODE", Type.GetType("System.String"));
            #endregion

            try
            {
                #region 请求数据
                System.Text.StringBuilder loStr = new System.Text.StringBuilder();
                //loStr.Append("cardState=").Append(cardState);
                string jsonQuery = WebWork.SendRequest(string.Empty, WebWork.URL_GetChannel);
                if (string.IsNullOrEmpty(jsonQuery))
                {
                    MsgBox.Warn(WebWork.RESULT_NULL);
                    //LogHelper.InfoLog(WebWork.RESULT_NULL);
                    return tblDatas;
                }
                #endregion

                #region 正常错误处理

                JsonGetChannel bill = JsonConvert.DeserializeObject<JsonGetChannel>(jsonQuery);
                if (bill == null)
                {
                    MsgBox.Warn(WebWork.JSON_DATA_NULL);
                    return tblDatas;
                }
                if (bill.flag != 0)
                {
                    MsgBox.Warn(bill.error);
                    return tblDatas;
                }
                #endregion

                #region 赋值
                foreach (JsonGetChannelResult tm in bill.result)
                {
                    DataRow newRow;
                    newRow = tblDatas.NewRow();
                    newRow["CH_CODE"] = tm.chCode;
                    newRow["CH_NAME"] = tm.chName;
                    newRow["BAK_CH_NAME"] = tm.bakChName;
                    newRow["IS_ACTIVE"] = tm.isActive;
                    newRow["BAK_CH_CODE"] = tm.bakChCode;
                    tblDatas.Rows.Add(newRow);
                }
                return tblDatas;
                #endregion
            }
            catch (Exception ex)
            {
                MsgBox.Err(ex.Message);
            }
            return tblDatas;
        }

        private void BindChannel(LocationEntity locEntity)
        {
            
            //DataTable dtBindChannel = new DataTable();
            //dtBindChannel.Columns.Add("CH_CODE",Type.GetType("System.Int32"));
            //dtBindChannel.Columns.Add("CH_NAME", Type.GetType("System.String"));
            dtChannel = GetChannel();
            //DataRow row;
            //foreach (DataRow rows in dtChannel.Rows)
            //{
            //    row = dtBindChannel.NewRow();
               
            //    if (rows["IS_ACTIVE"].ToString() == "Y")
            //    {
            //        row["CH_CODE"] = rows["CH_CODE"];
            //        row["CH_NAME"] = rows["CH_NAME"];
            //        dtBindChannel.Rows.Add(row);
            //    }
            //}
            //foreach (DataRow rows in dtChannel.Rows)
            //{
            //    row = dtBindChannel.NewRow();
            //    if (rows["IS_ACTIVE"].ToString() == "N")
            //    {
            //        DataRow[] selDataRow = dtBindChannel.Select(string.Format("CH_CODE='{0}'", rows["BAK_CH_CODE"]));
            //        if (selDataRow.Length <= 0)
            //        {
            //            row["CH_CODE"] = rows["BAK_CH_CODE"];
            //            row["CH_NAME"] = rows["BAK_CH_NAME"];
            //            dtBindChannel.Rows.Add(row);
            //        }
                    
            //    }
            //}

            if (dtChannel != null && dtChannel.Rows.Count > 0)
            {
                lookUpChannel.Properties.ValueMember = "CH_CODE";
                lookUpChannel.Properties.DisplayMember = "CH_CODE";
                lookUpChannel.Properties.DataSource = dtChannel;
                if (locEntity != null)
                {
                    lookUpChannel.EditValue = locEntity.Ch_Code;
                }
            }
        }
        private void btnSaveClose_Click(object sender, EventArgs e)
        {
            if (Save())
                this.DialogResult = DialogResult.OK;
        }

        public LocationEntity PrepareSave()
        {
            LocationEntity editEntity = locationEntity;
            if (editEntity == null)
            {
                editEntity = new LocationEntity();
                editEntity.WarehouseCode = GlobeSettings.LoginedUser.WarehouseCode;
                editEntity.WarehouseName = GlobeSettings.LoginedUser.WarehouseName;
            }

            editEntity.LocationCode = txtCode.Text.Trim();
            editEntity.LocationName = txtName.Text.Trim();
            ZoneEntity zone = listZones.Properties.GetDataSourceRowByKeyValue(listZones.EditValue) as ZoneEntity;
            editEntity.ZoneCode = zone.ZoneCode;
            editEntity.PassageCode = txtPassage.Text.Trim();
            editEntity.ShelfCode = txtShelf.Text.Trim();
            editEntity.FloorCode = txtFloor.Text.Trim();
            editEntity.CellCode = txtCellCode.Text.Trim();
            editEntity.SortOrder = (int)spinSortOrder.Value;
            editEntity.ZoneName = zone.ZoneName;

            if (listUnitGroup.EditValue != null)
            {
                editEntity.GrpCode = ConvertUtil.ToString(listUnitGroup.EditValue);
                editEntity.GrpName = listUnitGroup.Text;
            }
            else
            {
                editEntity.GrpCode = null;
                editEntity.GrpName = null;
            }

            if (listUnits.EditValue != null)
            {
                editEntity.UnitCode = ConvertUtil.ToString(listUnits.EditValue);
                editEntity.UnitName = listUnits.Text;
            }
            else
            {
                editEntity.UnitCode = editEntity.UnitName = null;
            }
         
            editEntity.LowerSize = (int)spinLowerSize.Value;
            editEntity.UpperSize = (int)spinUpperSize.Value;
            editEntity.IsActive = comboIsActive.Text;
            if (lookUpChannel.EditValue != null)
            {
                editEntity.Ch_Code = ConvertUtil.ToInt(lookUpChannel.EditValue);
                foreach(DataRow row in dtChannel.Rows)
                {
                    if (row["CH_CODE"].ToString() == editEntity.Ch_Code.ToString())
                    {
                        if (row["IS_ACTIVE"].ToString() == "Y")
                        {
                            editEntity.Ch_Name = row["CH_NAME"].ToString();
                        }
                        else
                        {
                            editEntity.Ch_Name = row["BAK_CH_NAME"].ToString();
                        }
                    }
                }
            }
            else
            {
                editEntity.Ch_Name = "";
            }

            return editEntity;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (Save())
            {
                if (locationEntity == null)
                    Continue();
                else
                    this.DialogResult = DialogResult.OK;
            }
        }

        private void OnLookUpEditButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            LookUpEdit editor = sender as LookUpEdit;
            if (e.Button.Kind == DevExpress.XtraEditors.Controls.ButtonPredefines.Delete)
            {
                editor.EditValue = null;
            }
        }

        private void listUnitGroup_EditValueChanged(object sender, EventArgs e)
        {
            if (listUnitGroup.EditValue != null)
            {
                string grpCode = ConvertUtil.ToString(listUnitGroup.EditValue);
                listUnits.Properties.DataSource = new UnitGroupDal().GetItemsByGrpCode(grpCode);//表不存在
            }
            else
            {
                listUnits.Properties.DataSource = null;
            }
        }

        private void lookUpChannel_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            LookUpEdit editor = sender as LookUpEdit;
            if (e.Button.Kind == DevExpress.XtraEditors.Controls.ButtonPredefines.Delete)
            {
                editor.EditValue = null;
            }
        }
    }
}