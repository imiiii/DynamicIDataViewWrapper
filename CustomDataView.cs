using Jint.Parser;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hooshkar.Fennec.AI.Common.TrainingInterface
{
    public class CustomDataView : IDataView
    {
        private readonly List<Single[]> _data;
        private readonly string[] _keys;
        public List<Single[]> Data
        {
            get
            {
                return _data;
            }
        }
        public CustomDataView(List<Single[]> data, string[] keys)
        {
            _data = data;
            _keys = keys;


            var builder = new DataViewSchema.Builder();
            foreach (string key in keys)
            {
                builder.AddColumn(key, NumberDataViewType.Single);
            }
            Schema = builder.ToSchema();
        }

        public Single[] this[int index]
        {
            get { return _data[index]; }
            set { _data[index] = value; }
        }

        public int Count
        {
            get { return _data.Count; }
        }

        public object SyncRoot
        {
            get { return this; }
        }

        public bool CanShuffle => false;

        public DataViewSchema Schema { get; }
        //public object Data { get; internal set; }

        public void AddRow(Single[] row)
        {
            _data.Add(row);
        }

        public void Flush()
        {
            // No-op
        }

        public long? GetRowCount() => null;
        public DataViewRowCursor GetRowCursor(
               IEnumerable<DataViewSchema.Column> columnsNeeded,
               Random rand = null)
        {

            return new Curser(this, _data, _keys);
        }

        public DataViewRowCursor[] GetRowCursorSet(
            IEnumerable<DataViewSchema.Column> columnsNeeded, int n,
            Random rand = null)

            => new[] { GetRowCursor(columnsNeeded, rand) };
    }



    class Curser : DataViewRowCursor
    {
        private readonly IEnumerable<Single[]> data;
        private readonly IEnumerable<string> keys;
        private readonly long batch;
        private int position;
        private List<Single[]>.Enumerator enumerator;
        private Delegate[] getters;
        private bool disposed;

        public Curser(CustomDataView customDataView, IEnumerable<Single[]> data, IEnumerable<string> keys)
        {
            this.data = data;
            this.keys = keys;
            Schema = customDataView.Schema;
            batch = 0;
            position = -1;

            enumerator = customDataView.Data.GetEnumerator();
            getters = new Delegate[]
                   {
                       
                    
                            (ValueGetter<float>) GetterField0,
                            
                   };
        }


        private void GetterField0(ref float value) => value = enumerator.Current.ElementAt(position);
       

        public override long Position => position;
        public override long Batch => batch;

        public override DataViewSchema Schema { get; }

        public override ValueGetter<TValue> GetGetter<TValue>(DataViewSchema.Column column)
        {

            if (!IsColumnActive(column))
                throw new ArgumentOutOfRangeException(nameof(column));
            return (ValueGetter<TValue>)getters[column.Index];
        }

        public override ValueGetter<DataViewRowId> GetIdGetter()
        {
            throw new NotImplementedException();
        }

        public override bool IsColumnActive(DataViewSchema.Column column)
        {
            return true;
        }

        public override bool MoveNext()
        {
            if (disposed)
                return false;
            
            if (enumerator.MoveNext())
            {
                position++;
                return true;
            }
            Dispose();
            return false;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                enumerator.Dispose();
                position = -1;
            }
            disposed = true;
            base.Dispose(disposing);
        }
    }
}
