#pragma once
#include "ProjObject.h"

namespace SharpProj {
	ref class GeometricCoordinateReferenceSystem;
	ref class ProjDatum;
	ref class ProjDatumList;
	ref class CoordinateSystem;
	ref class Ellipsoid;
	ref class PrimeMeridian;
	ref class CoordinateOperation;

	public ref class CoordinateReferenceSystem : ProjObject
	{
	private:
		ProjContext^ m_ctx;
		CoordinateSystem^ m_cs;

		~CoordinateReferenceSystem()
		{

		}

	internal:
		CoordinateReferenceSystem(ProjContext^ ctx, PJ* pj)
			: ProjObject(ctx, pj)
		{
		}

	public:
		property bool IsDeprecated
		{
			bool get()
			{
				return proj_is_deprecated(this);
			}
		}

		property CoordinateSystem^ CoordinateSystem
		{
			SharpProj::CoordinateSystem^ get()
			{
				if (!m_cs)
					m_cs = GetCoordinateSystem(nullptr);

				return m_cs;
			}
		}

	public:
		CoordinateReferenceSystem^ Clone([Optional]ProjContext^ ctx)
		{
			return static_cast<CoordinateReferenceSystem^>(__super::Clone(ctx));
		}

	public:
		CoordinateReferenceSystem^ GetNormalized([Optional] ProjContext^ context);
		CoordinateReferenceSystem^ GetGeodeticCoordinateReferenceSystem([Optional] ProjContext^ context);
		ProjDatum^ GetHorizontalDatum([Optional] ProjContext^ context);
		ProjDatum^ GetDatum([Optional] ProjContext^ context);
		ProjDatumList^ GetDatumList([Optional] ProjContext^ context);
		ProjDatum^ GetDatumForced([Optional] ProjContext^ context);
		SharpProj::CoordinateSystem^ GetCoordinateSystem([Optional] ProjContext^ context);
		Ellipsoid^ GetEllipsoid([Optional] ProjContext^ context);
		PrimeMeridian^ GetPrimeMeridian([Optional] ProjContext^ context);
		CoordinateOperation^ GetCoordinateOperation([Optional] ProjContext^ context);
		CoordinateReferenceSystem^ GetBaseCoordinateReferenceSystem([Optional] ProjContext^ context);
		CoordinateReferenceSystem^ GetHubCoordinateReferenceSystem([Optional] ProjContext^ context);

	public:
		static CoordinateReferenceSystem^ Create(String^ from, [Optional] ProjContext^ ctx);
		static CoordinateReferenceSystem^ Create(array<String^>^ from, [Optional] ProjContext^ ctx);
	};
}
