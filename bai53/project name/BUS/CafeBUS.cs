using System;
using System.Data;
using CafePOSSystem.DAL;

namespace CafePOSSystem.BUS
{
    public class CafeBUS
    {
        private readonly CafeDAL dal = new CafeDAL();
        public DataTable GetTables() => dal.GetTables();
        public DataTable GetCategories() => dal.GetCategories();
        public DataTable GetMenu(int categoryId) => dal.GetMenu(categoryId);
        public DataTable GetOrder(int tableId) => dal.GetOrder(tableId);

        public void AddItem(int tableId, int itemId, int quantity)
        {
            if (tableId <= 0 || itemId <= 0) throw new ArgumentException("Hãy chọn bàn và món ăn.");
            if (quantity <= 0) throw new ArgumentException("Số lượng phải lớn hơn 0.");
            dal.AddItem(tableId, itemId, quantity);
        }

        public decimal Pay(int tableId)
        {
            if (tableId <= 0) throw new ArgumentException("Hãy chọn bàn.");
            return dal.Pay(tableId);
        }
    }
}
